using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTrip.App.Client.Pages
{
    public partial class Dashboard
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        private bool _isLoading = true;
        private List<BookingResponseModel> _allUnpaid = new();
        private string _filter = "All";

        private int _totalUnpaid;
        private int _busCount;
        private int _hotelCount;
        private int _pkgCount;
        private decimal _totalUnpaidAmount;

        private string _barChartMode = "count";
        private string[] _chartLabels = { "Bus", "Hotel", "Package" };
        private List<ChartSeries<double>> _barSeries = new();
        private List<ChartSeries<double>> _donutSeries = new();
        private readonly ChartOptions _barChartOptions = new()
        {
            ShowLegend = true,
            ChartPalette = new[] { "#14B8A6", "#F9A8D4", "#94A3B8" }
        };
        private readonly ChartOptions _donutChartOptions = new()
        {
            ShowLegend = true,
            ChartPalette = new[] { "#6366F1", "#38BDF8", "#34D399", "#FBBF24" }
        };

        private IEnumerable<BookingResponseModel> FilteredItems => _filter switch
        {
            "Bus" => _allUnpaid.Where(x => x.BookingType == "Bus"),
            "Hotel" => _allUnpaid.Where(x => x.BookingType == "Hotel"),
            "Package" => _allUnpaid.Where(x => x.BookingType is "Package" or "TravelPackage"),
            _ => _allUnpaid
        };

        protected override async Task OnInitializedAsync() => await LoadDataAsync();

        private async Task LoadDataAsync()
        {
            _isLoading = true;
            StateHasChanged();
            try
            {
                var url = $"api/booking?pageNo=1&pageSize=500&paymentStatus={(int)PaymentStatus.Unpaid}&newestFirst=true";
                var response = await Http.GetFromJsonAsync<PaginationResponse<BookingResponseModel>>(url);
                _allUnpaid = response?.Data.ToList() ?? new();
                RecalcStats();
                UpdateCharts();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Dashboard: " + ex.Message);
                _allUnpaid = new();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void RecalcStats()
        {
            _totalUnpaid = _allUnpaid.Count;
            _busCount = _allUnpaid.Count(x => x.BookingType == "Bus");
            _hotelCount = _allUnpaid.Count(x => x.BookingType == "Hotel");
            _pkgCount = _allUnpaid.Count(x => x.BookingType is "Package" or "TravelPackage");
            _totalUnpaidAmount = _allUnpaid.Sum(x => x.TotalAmount);
        }

        private void SetFilter(string filter) => _filter = filter;

        private void OnDashboardRowClick(TableRowClickEventArgs<BookingResponseModel> args)
            => Nav.NavigateTo("/bookings?status=1");

        private void UpdateCharts()
        {
            if (_barChartMode == "amount")
            {
                _barSeries = new List<ChartSeries<double>>
                {
                    new() { Name = "Pending", Data = TypeAmountByBookingStatus(BookingStatusCodes.Pending) },
                    new() { Name = "Confirmed", Data = TypeAmountByBookingStatus(BookingStatusCodes.Confirmed) },
                    new() { Name = "Rejected", Data = TypeAmountByBookingStatus(BookingStatusCodes.Rejected) }
                };
            }
            else
            {
                _barSeries = new List<ChartSeries<double>>
                {
                    new() { Name = "Pending", Data = TypeCountByBookingStatus(BookingStatusCodes.Pending) },
                    new() { Name = "Confirmed", Data = TypeCountByBookingStatus(BookingStatusCodes.Confirmed) },
                    new() { Name = "Rejected", Data = TypeCountByBookingStatus(BookingStatusCodes.Rejected) }
                };
            }

            _donutSeries = new List<ChartSeries<double>>
            {
                new() { Name = "Bus", Data = new[] { (double)Math.Max(0, _busCount) } },
                new() { Name = "Hotel", Data = new[] { (double)Math.Max(0, _hotelCount) } },
                new() { Name = "Package", Data = new[] { (double)Math.Max(0, _pkgCount) } }
            };
        }

        private double[] TypeCountByBookingStatus(int status) => new double[]
        {
            _allUnpaid.Count(x => x.BookingType == "Bus" && x.BookingStatusCode == status),
            _allUnpaid.Count(x => x.BookingType == "Hotel" && x.BookingStatusCode == status),
            _allUnpaid.Count(x => x.BookingType is "Package" or "TravelPackage" && x.BookingStatusCode == status)
        };

        private double[] TypeAmountByBookingStatus(int status) => new[]
        {
            (double)_allUnpaid.Where(x => x.BookingType == "Bus" && x.BookingStatusCode == status).Sum(x => x.TotalAmount),
            (double)_allUnpaid.Where(x => x.BookingType == "Hotel" && x.BookingStatusCode == status).Sum(x => x.TotalAmount),
            (double)_allUnpaid.Where(x => x.BookingType is "Package" or "TravelPackage" && x.BookingStatusCode == status).Sum(x => x.TotalAmount)
        };

        private static string GetBookedOn(BookingResponseModel b)
        {
            var d = b.CreatedAt ?? b.BookingDate;
            return d.HasValue ? d.Value.ToLocalTime().ToString("dd MMM yyyy HH:mm") : "—";
        }

        private static string GetServiceShort(BookingResponseModel b)
        {
            if (b.Details == null) return b.ItemName ?? "—";
            return b.BookingType switch
            {
                "Bus" => b.Details.BusName ?? b.Details.BusNumber ?? "—",
                "Hotel" => $"{b.Details.HotelName ?? "—"} · {b.Details.HotelRoomTypeName ?? "Room"}",
                _ => b.Details.PackageName ?? b.ItemName ?? "—"
            };
        }
    }
}
