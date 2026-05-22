using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Reports
{
    public class ReportService : IReportService
    {
        private const int PaidStatus = 2;
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SalesRevenueReportModel> GetSalesRevenueReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var (start, end) = NormalizeDateRange(startDate, endDate);
            var periodDays = (end.Date - start.Date).Days + 1;
            var prevStart = start.AddDays(-periodDays);
            var prevEnd = start.AddDays(-1);

            var paidBookings = await _context.Bookings
                .Where(b => b.DeletedAt == null && b.PaymentStatus == PaidStatus)
                .ToListAsync();

            var inPeriod = paidBookings.Where(b => InRange(GetBookingDate(b), start, end)).ToList();
            var prevPeriod = paidBookings.Where(b => InRange(GetBookingDate(b), prevStart, prevEnd)).ToList();

            var totalRevenue = inPeriod.Sum(b => b.TotalAmount);
            var prevRevenue = prevPeriod.Sum(b => b.TotalAmount);

            var busRevenue = inPeriod.Where(b => b.BookingType == "Bus").Sum(b => b.TotalAmount);
            var hotelRevenue = inPeriod.Where(b => b.BookingType == "Hotel").Sum(b => b.TotalAmount);
            var packageRevenue = inPeriod.Where(b => IsPackageType(b.BookingType)).Sum(b => b.TotalAmount);

            var report = new SalesRevenueReportModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = totalRevenue,
                PreviousPeriodRevenue = prevRevenue,
                RevenueGrowthPercentage = CalcGrowth(totalRevenue, prevRevenue),
                PaidTransactionCount = inPeriod.Count
            };

            if (totalRevenue > 0)
            {
                report.RevenueByType = new List<RevenueByTypeItem>
                {
                    new() { Type = "Bus", Amount = busRevenue, Percentage = RoundPct(busRevenue, totalRevenue), TransactionCount = inPeriod.Count(b => b.BookingType == "Bus") },
                    new() { Type = "Hotel", Amount = hotelRevenue, Percentage = RoundPct(hotelRevenue, totalRevenue), TransactionCount = inPeriod.Count(b => b.BookingType == "Hotel") },
                    new() { Type = "Package", Amount = packageRevenue, Percentage = RoundPct(packageRevenue, totalRevenue), TransactionCount = inPeriod.Count(b => IsPackageType(b.BookingType)) }
                };
            }

            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                var day = DateOnly.FromDateTime(d);
                report.ChartLabels.Add(d.ToString("dd MMM"));
                var dayBookings = inPeriod.Where(b => DateOnly.FromDateTime(GetBookingDate(b)) == day).ToList();
                report.BusSalesSeries.Add((double)dayBookings.Where(b => b.BookingType == "Bus").Sum(b => b.TotalAmount));
                report.HotelSalesSeries.Add((double)dayBookings.Where(b => b.BookingType == "Hotel").Sum(b => b.TotalAmount));
                report.PackageSalesSeries.Add((double)dayBookings.Where(b => IsPackageType(b.BookingType)).Sum(b => b.TotalAmount));
                report.TotalSalesSeries.Add((double)dayBookings.Sum(b => b.TotalAmount));
            }

            return report;
        }

        public async Task<BookingAnalyticsReportModel> GetBookingAnalyticsReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var (start, end) = NormalizeDateRange(startDate, endDate);

            var bookings = await _context.Bookings
                .Where(b => b.DeletedAt == null)
                .ToListAsync();

            var inPeriod = bookings.Where(b => InRange(GetBookingDate(b), start, end)).ToList();

            var report = new BookingAnalyticsReportModel
            {
                StartDate = start,
                EndDate = end,
                TotalBookings = inPeriod.Count,
                ConfirmedCount = inPeriod.Count(b => b.BookingStatus == 2),
                PendingCount = inPeriod.Count(b => b.BookingStatus == 1),
                RejectedCount = inPeriod.Count(b => b.BookingStatus == 3),
                PaidCount = inPeriod.Count(b => b.PaymentStatus == 2),
                UnpaidCount = inPeriod.Count(b => b.PaymentStatus == 0 || b.PaymentStatus == null),
                UnderReviewCount = inPeriod.Count(b => b.PaymentStatus == 1),
                BusCount = inPeriod.Count(b => b.BookingType == "Bus"),
                HotelCount = inPeriod.Count(b => b.BookingType == "Hotel"),
                PackageCount = inPeriod.Count(b => IsPackageType(b.BookingType)),
                TotalBookingValue = inPeriod.Sum(b => b.TotalAmount),
                AverageBookingAmount = inPeriod.Count > 0 ? Math.Round(inPeriod.Average(b => b.TotalAmount), 0) : 0,
                ConversionRate = inPeriod.Count > 0
                    ? Math.Round(100.0 * inPeriod.Count(b => b.BookingStatus == 2) / inPeriod.Count, 1)
                    : 0
            };

            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                var day = DateOnly.FromDateTime(d);
                var dayBookings = inPeriod.Where(b => DateOnly.FromDateTime(GetBookingDate(b)) == day).ToList();
                report.ChartLabels.Add(d.ToString("dd MMM"));
                report.BookingVolumeSeries.Add(dayBookings.Count);
                report.ConfirmedVolumeSeries.Add(dayBookings.Count(b => b.BookingStatus == 2));
                report.PendingVolumeSeries.Add(dayBookings.Count(b => b.BookingStatus == 1));
            }

            return report;
        }

        public async Task<TopServicesReportModel> GetTopServicesReportAsync(
            DateTime? startDate, DateTime? endDate, string? serviceType, int top, string? metric)
        {
            var (start, end) = NormalizeDateRange(startDate, endDate);
            top = Math.Clamp(top, 1, 50);
            var sortByRevenue = !string.Equals(metric, "bookings", StringComparison.OrdinalIgnoreCase);
            var typeFilter = string.IsNullOrWhiteSpace(serviceType) ? "All" : serviceType.Trim();

            var paidBookings = await _context.Bookings
                .Include(b => b.BookingDetails)
                .Where(b => b.DeletedAt == null && b.PaymentStatus == PaidStatus)
                .ToListAsync();

            var inPeriod = paidBookings.Where(b => InRange(GetBookingDate(b), start, end)).ToList();

            var busIds = new HashSet<long>();
            var roomIds = new HashSet<long>();
            var pkgIds = new HashSet<long>();

            foreach (var booking in inPeriod)
            {
                var detail = booking.BookingDetails.FirstOrDefault();
                if (booking.BookingType == "Bus")
                {
                    var id = detail?.BusId ?? booking.ItemId;
                    if (id.HasValue) busIds.Add(id.Value);
                }
                else if (booking.BookingType == "Hotel")
                {
                    var id = detail?.HotelRoomId ?? booking.ItemId;
                    if (id.HasValue) roomIds.Add(id.Value);
                }
                else if (IsPackageType(booking.BookingType))
                {
                    var id = detail?.PackageId ?? booking.ItemId;
                    if (id.HasValue) pkgIds.Add(id.Value);
                }
            }

            var busesMap = busIds.Any()
                ? await _context.Buses.Where(b => busIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id)
                : new Dictionary<long, DotNet8.EasyTripBackendApi.DbService.Models.Bus>();
            var roomsMap = roomIds.Any()
                ? await _context.HotelRooms.Include(r => r.Hotel).Include(r => r.RoomType)
                    .Where(r => roomIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id)
                : new Dictionary<long, DotNet8.EasyTripBackendApi.DbService.Models.HotelRoom>();
            var packagesMap = pkgIds.Any()
                ? await _context.TravelPackages.Where(p => pkgIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id)
                : new Dictionary<long, DotNet8.EasyTripBackendApi.DbService.Models.TravelPackage>();

            var aggregates = new Dictionary<string, (long Id, string Name, string Type, string? Desc, int Count, decimal Revenue)>();

            void AddService(string key, long id, string name, string type, string? desc, decimal amount)
            {
                if (aggregates.TryGetValue(key, out var existing))
                    aggregates[key] = (existing.Id, existing.Name, existing.Type, existing.Desc, existing.Count + 1, existing.Revenue + amount);
                else
                    aggregates[key] = (id, name, type, desc, 1, amount);
            }

            foreach (var booking in inPeriod)
            {
                var detail = booking.BookingDetails.FirstOrDefault();
                var amount = booking.TotalAmount;

                if (booking.BookingType == "Bus" && (typeFilter == "All" || typeFilter == "Bus"))
                {
                    var busId = detail?.BusId ?? booking.ItemId;
                    if (busId.HasValue)
                    {
                        busesMap.TryGetValue(busId.Value, out var bus);
                        var name = bus != null ? $"{bus.BusName} ({bus.BusNumber})" : $"Bus #{busId}";
                        var desc = bus != null ? $"{bus.StartPoint} → {bus.EndPoint}" : null;
                        AddService($"Bus-{busId}", busId.Value, name, "Bus", desc, amount);
                    }
                }
                else if (booking.BookingType == "Hotel" && (typeFilter == "All" || typeFilter == "Hotel"))
                {
                    var roomId = detail?.HotelRoomId ?? booking.ItemId;
                    if (roomId.HasValue)
                    {
                        roomsMap.TryGetValue(roomId.Value, out var room);
                        var name = room != null
                            ? $"{room.Hotel?.HotelName ?? "Hotel"} — {room.RoomType?.TypeName ?? "Room"}"
                            : $"Room #{roomId}";
                        var desc = room?.Hotel?.Location;
                        AddService($"Hotel-{roomId}", roomId.Value, name, "Hotel", desc, amount);
                    }
                }
                else if (IsPackageType(booking.BookingType) && (typeFilter == "All" || typeFilter == "Package"))
                {
                    var pkgId = detail?.PackageId ?? booking.ItemId;
                    if (pkgId.HasValue)
                    {
                        packagesMap.TryGetValue(pkgId.Value, out var pkg);
                        var name = pkg?.PackageName ?? $"Package #{pkgId}";
                        var desc = pkg != null ? $"{pkg.DurationDays} days" : null;
                        AddService($"Package-{pkgId}", pkgId.Value, name, "Package", desc, amount);
                    }
                }
            }

            var ordered = sortByRevenue
                ? aggregates.OrderByDescending(x => x.Value.Revenue)
                : aggregates.OrderByDescending(x => x.Value.Count);

            var report = new TopServicesReportModel
            {
                StartDate = start,
                EndDate = end,
                ServiceTypeFilter = typeFilter,
                Metric = sortByRevenue ? "revenue" : "bookings"
            };

            var rank = 1;
            foreach (var item in ordered.Take(top))
            {
                var v = item.Value;
                report.Services.Add(new TopServiceItem
                {
                    Rank = rank++,
                    ServiceId = v.Id,
                    ServiceName = v.Name,
                    ServiceType = v.Type,
                    Description = v.Desc,
                    BookingCount = v.Count,
                    TotalRevenue = v.Revenue,
                    AverageOrderValue = v.Count > 0 ? Math.Round(v.Revenue / v.Count, 0) : 0
                });
            }

            return report;
        }

        private static (DateTime start, DateTime end) NormalizeDateRange(DateTime? startDate, DateTime? endDate)
        {
            var end = (endDate ?? DateTime.UtcNow).Date;
            var start = (startDate ?? end.AddDays(-29)).Date;
            if (start > end) (start, end) = (end, start);
            return (start, end.AddDays(1).AddTicks(-1));
        }

        private static DateTime GetBookingDate(Booking b) =>
            b.CreatedAt ?? b.BookingDate ?? DateTime.UtcNow;

        private static bool InRange(DateTime date, DateTime start, DateTime end) =>
            date >= start && date <= end;

        private static bool IsPackageType(string type) =>
            type == "Package" || type == "TravelPackage";

        private static double CalcGrowth(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100.0 : 0.0;
            return Math.Round((double)((current - previous) / previous) * 100, 1);
        }

        private static double RoundPct(decimal part, decimal total) =>
            total == 0 ? 0 : Math.Round((double)(part / total) * 100, 1);
    }
}
