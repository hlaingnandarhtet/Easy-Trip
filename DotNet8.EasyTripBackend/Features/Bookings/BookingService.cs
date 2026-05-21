using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        private static List<string> ParseSeatList(string? seats) =>
            string.IsNullOrWhiteSpace(seats)
                ? new List<string>()
                : seats.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToUpperInvariant())
                    .ToList();

        private async Task<List<string>> GetOccupiedSeatsAsync(long busId, DateOnly travelDate, long? excludeBookingId = null)
        {
            var seatStrings = await (
                from bd in _context.BookingDetails
                join b in _context.Bookings on bd.BookingId equals b.Id
                where bd.BusId == busId
                      && bd.TravelDate == travelDate
                      && b.DeletedAt == null
                      && b.BookingStatus != null
                      && BookingStatusCodes.SeatBlockingStatuses.Contains(b.BookingStatus.Value)
                      && (excludeBookingId == null || b.Id != excludeBookingId.Value)
                select bd.SelectedSeats
            ).ToListAsync();

            return seatStrings
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .SelectMany(s => s!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();
        }

        private static void EnsureNoSeatConflict(IReadOnlyList<string> requested, IReadOnlyList<string> occupied)
        {
            var conflicting = requested
                .Intersect(occupied, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (conflicting.Count > 0)
            {
                throw new Exception(
                    $"The following seats have just been booked by another user: {string.Join(", ", conflicting)}. Please select different seats.");
            }
        }

        public async Task<PaginationResponse<BookingResponseModel>> GetBookingsAsync(
            int pageNo,
            int pageSize,
            string? name = null,
            string? type = null,
            int? status = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                .Where(b => b.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(b => b.User != null && (b.User.Name.Contains(name) || b.User.Email.Contains(name)));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(b => b.BookingType == type);
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.BookingStatus == status.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(b => b.TravelDate >= startDate.Value && b.TravelDate <= endDate.Value);
            }
            else if (startDate.HasValue)
            {
                query = query.Where(b => b.TravelDate >= startDate.Value);
            }
            else if (endDate.HasValue)
            {
                query = query.Where(b => b.TravelDate <= endDate.Value);
            }

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderBy(b => b.Id)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedList = await MapBookingsAsync(bookings);

            return new PaginationResponse<BookingResponseModel>(mappedList, totalCount, pageNo, pageSize);
        }

        public async Task<BookingResponseModel?> GetBookingByIdAsync(long id)
        {
            return await GetBookingAsync(id);
        }

        public async Task<BookingResponseModel?> GetBookingAsync(long id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);

            if (booking == null) return null;

            var mapped = await MapBookingsAsync(new List<Booking> { booking });
            return mapped.FirstOrDefault();
        }

        public async Task<BookingResponseModel> CreateBookingAsync(BookingRequestModel request)
        {
            // Validate User
            if (request.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId.Value && u.DeletedAt == null);
                if (!userExists) throw new Exception("User not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Business logic check based on BookingType
                if (request.BookingType.Equals("Hotel", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.HotelRoomId.HasValue)
                        throw new Exception("HotelRoomId is required for Hotel bookings.");

                    if (request.Quantity <= 0)
                        throw new Exception("Quantity of rooms booked must be greater than zero.");

                    if (!request.EndDate.HasValue)
                        throw new Exception("Check-out EndDate is required for Hotel bookings.");

                    // Retrieve hotel room and check availability
                    var room = await _context.HotelRooms
                        .FirstOrDefaultAsync(hr => hr.Id == request.HotelRoomId.Value && hr.DeletedAt == null);

                    if (room == null)
                        throw new Exception("Hotel Room not found");

                    if (room.AvailableRooms < request.Quantity)
                        throw new Exception($"Requested quantity ({request.Quantity}) exceeds available rooms ({room.AvailableRooms}).");

                    // Decrease available_rooms inside hotel_rooms table
                    room.AvailableRooms -= request.Quantity;

                    // If available_rooms reaches 0, update hotel_status to 1 (Full)
                    if (room.AvailableRooms == 0)
                    {
                        room.HotelStatus = 1; // 1: Full
                    }

                    room.UpdatedAt = DateTime.UtcNow;
                    _context.HotelRooms.Update(room);
                }
                else if (request.BookingType.Equals("Bus", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.BusId.HasValue)
                        throw new Exception("BusId is required for Bus bookings.");

                    var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId.Value && b.DeletedAt == null);
                    if (!busExists)
                        throw new Exception("Bus not found");

                    if (!string.IsNullOrEmpty(request.SelectedSeats))
                    {
                        var requestedSeats = ParseSeatList(request.SelectedSeats);
                        var occupiedSeats = await GetOccupiedSeatsAsync(request.BusId.Value, request.TravelDate);
                        EnsureNoSeatConflict(requestedSeats, occupiedSeats);
                    }
                }
                else if (request.BookingType.Equals("Package", StringComparison.OrdinalIgnoreCase) || 
                         request.BookingType.Equals("TravelPackage", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.PackageId.HasValue)
                        throw new Exception("PackageId is required for Package bookings.");

                    var packageExists = await _context.TravelPackages.AnyAsync(tp => tp.Id == request.PackageId.Value && tp.DeletedAt == null);
                    if (!packageExists)
                        throw new Exception("Travel Package not found");
                }
                else
                {
                    throw new Exception("Invalid BookingType. Must be 'Bus', 'Hotel', or 'Package'");
                }

                // Insert into bookings table
                var newBooking = new Booking
                {
                    UserId = request.UserId,
                    BookingType = request.BookingType,
                    ItemId = request.HotelRoomId ?? request.BusId ?? request.PackageId,
                    BookingDate = DateTime.UtcNow,
                    TravelDate = request.TravelDate,
                    TotalAmount = request.TotalAmount,
                    PaymentStatus = (int)request.PaymentStatus,
                    BookingStatus = (int)request.BookingStatus,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                // Insert into booking_details table
                var newBookingDetail = new BookingDetail
                {
                    BookingId = newBooking.Id,
                    BusId = request.BusId,
                    HotelRoomId = request.HotelRoomId,
                    PackageId = request.PackageId,
                    SelectedSeats = request.SelectedSeats,
                    Quantity = request.Quantity,
                    TravelDate = request.TravelDate,
                    EndDate = request.EndDate
                };

                _context.BookingDetails.Add(newBookingDetail);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return await GetBookingByIdAsync(newBooking.Id) ?? throw new Exception("Failed to retrieve created booking details.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BookingResponseModel?> UpdateBookingAsync(long id, BookingRequestModel request)
        {
            var existingBooking = await _context.Bookings
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);

            if (existingBooking == null) return null;

            // Validate User
            if (request.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId.Value && u.DeletedAt == null);
                if (!userExists) throw new Exception("User not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (request.BookingType.Equals("Hotel", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.HotelRoomId.HasValue)
                        throw new Exception("HotelRoomId is required for Hotel bookings.");

                    var roomExists = await _context.HotelRooms.AnyAsync(hr => hr.Id == request.HotelRoomId.Value && hr.DeletedAt == null);
                    if (!roomExists)
                        throw new Exception("Hotel Room not found");
                }
                else if (request.BookingType.Equals("Bus", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.BusId.HasValue)
                        throw new Exception("BusId is required for Bus bookings.");

                    var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId.Value && b.DeletedAt == null);
                    if (!busExists)
                        throw new Exception("Bus not found");
                }
                else if (request.BookingType.Equals("Package", StringComparison.OrdinalIgnoreCase) || 
                         request.BookingType.Equals("TravelPackage", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.PackageId.HasValue)
                        throw new Exception("PackageId is required for Package bookings.");

                    var packageExists = await _context.TravelPackages.AnyAsync(tp => tp.Id == request.PackageId.Value && tp.DeletedAt == null);
                    if (!packageExists)
                        throw new Exception("Travel Package not found");
                }
                else
                {
                    throw new Exception("Invalid BookingType. Must be 'Bus', 'Hotel', or 'Package'");
                }

                // Update Booking
                existingBooking.UserId = request.UserId;
                existingBooking.BookingType = request.BookingType;
                existingBooking.ItemId = request.HotelRoomId ?? request.BusId ?? request.PackageId;
                existingBooking.TravelDate = request.TravelDate;
                existingBooking.TotalAmount = request.TotalAmount;
                existingBooking.PaymentStatus = (int)request.PaymentStatus;
                existingBooking.BookingStatus = (int)request.BookingStatus;
                existingBooking.UpdatedAt = DateTime.UtcNow;

                _context.Bookings.Update(existingBooking);

                // Update or create booking details
                var detail = existingBooking.BookingDetails.FirstOrDefault();
                if (detail == null)
                {
                    detail = new BookingDetail { BookingId = existingBooking.Id };
                    _context.BookingDetails.Add(detail);
                }

                detail.BusId = request.BusId;
                detail.HotelRoomId = request.HotelRoomId;
                detail.PackageId = request.PackageId;
                detail.SelectedSeats = request.SelectedSeats;
                detail.Quantity = request.Quantity;
                detail.TravelDate = request.TravelDate;
                detail.EndDate = request.EndDate;

                if (detail.Id > 0)
                {
                    _context.BookingDetails.Update(detail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetBookingByIdAsync(id);
        }

        public async Task<BookingResponseModel?> UpdatePaymentAndBookingStatusAsync(long id, int paymentStatus, int bookingStatus)
        {
            var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
            if (existingBooking == null) return null;

            existingBooking.PaymentStatus = paymentStatus;
            existingBooking.BookingStatus = bookingStatus;
            existingBooking.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Update(existingBooking);
            await _context.SaveChangesAsync();

            return await GetBookingByIdAsync(id);
        }

        public async Task<bool> DeleteBookingAsync(long id)
        {
            var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
            if (existingBooking == null) return false;

            existingBooking.DeletedAt = DateTime.UtcNow;

            _context.Bookings.Update(existingBooking);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<string>> GetReservedSeatsForBusAsync(long busId, DateOnly travelDate)
        {
            return await GetOccupiedSeatsAsync(busId, travelDate);
        }

        public async Task<List<BookingResponseModel>> GetBookingsByPhoneAsync(string phone)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == phone && u.DeletedAt == null);

            if (user == null)
                return new List<BookingResponseModel>();

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                .Where(b => b.UserId == user.Id && b.DeletedAt == null)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return await MapBookingsAsync(bookings);
        }

        public async Task<BookingResponseModel> CreatePublicBookingAsync(PublicBookingRequestModel request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                // 1 & 2 & 3. Check Users table by Phone or Email to avoid unique constraint violations
                var user = await _context.Users.FirstOrDefaultAsync(u => (u.Phone == request.Phone || u.Email == request.Email) && u.DeletedAt == null);
                if (user == null)
                {
                    user = new DotNet8.EasyTripBackendApi.DbService.Models.User
                    {
                        Name = request.Name,
                        Phone = request.Phone,
                        Email = request.Email,
                        Role = "Customer",
                        AccountStatus = "Inactive/PendingApproval",
                        Password = "GeneratedPassword123!", // Dummy password for now or generated
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Business logic check based on BookingType
                if (request.BookingType.Equals("Hotel", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.HotelRoomId.HasValue)
                        throw new Exception("HotelRoomId is required for Hotel bookings.");
                    
                    var room = await _context.HotelRooms.FirstOrDefaultAsync(hr => hr.Id == request.HotelRoomId.Value && hr.DeletedAt == null);
                    if (room == null) throw new Exception("Hotel Room not found");
                    if (room.AvailableRooms < request.Quantity)
                        throw new Exception($"Requested quantity ({request.Quantity}) exceeds available rooms ({room.AvailableRooms}).");
                    
                    room.AvailableRooms -= request.Quantity;
                    if (room.AvailableRooms == 0) room.HotelStatus = 1;
                    room.UpdatedAt = DateTime.UtcNow;
                    _context.HotelRooms.Update(room);
                }
                else if (request.BookingType.Equals("Bus", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.BusId.HasValue) throw new Exception("BusId is required for Bus bookings.");
                    var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId.Value && b.DeletedAt == null);
                    if (!busExists) throw new Exception("Bus not found");

                    if (!string.IsNullOrEmpty(request.SelectedSeats))
                    {
                        var requestedSeats = ParseSeatList(request.SelectedSeats);
                        var occupiedSeats = await GetOccupiedSeatsAsync(request.BusId.Value, request.TravelDate);
                        EnsureNoSeatConflict(requestedSeats, occupiedSeats);
                    }
                }
                else if (request.BookingType.Equals("Package", StringComparison.OrdinalIgnoreCase) || request.BookingType.Equals("TravelPackage", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.PackageId.HasValue) throw new Exception("PackageId is required for Package bookings.");
                    var packageExists = await _context.TravelPackages.AnyAsync(tp => tp.Id == request.PackageId.Value && tp.DeletedAt == null);
                    if (!packageExists) throw new Exception("Travel Package not found");
                }
                else
                {
                    throw new Exception("Invalid BookingType. Must be 'Bus', 'Hotel', or 'Package'");
                }

                // 4. Create the Booking and BookingDetail
                var newBooking = new DotNet8.EasyTripBackendApi.DbService.Models.Booking
                {
                    UserId = user.Id,
                    BookingType = request.BookingType,
                    ItemId = request.HotelRoomId ?? request.BusId ?? request.PackageId,
                    BookingDate = DateTime.UtcNow,
                    TravelDate = request.TravelDate,
                    TotalAmount = request.TotalAmount,
                    PaymentStatus = (int)PaymentStatus.Unpaid,
                    BookingStatus = BookingStatusCodes.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                var newBookingDetail = new DotNet8.EasyTripBackendApi.DbService.Models.BookingDetail
                {
                    BookingId = newBooking.Id,
                    BusId = request.BusId,
                    HotelRoomId = request.HotelRoomId,
                    PackageId = request.PackageId,
                    SelectedSeats = request.SelectedSeats,
                    Quantity = request.Quantity,
                    TravelDate = request.TravelDate,
                    EndDate = request.EndDate
                };

                _context.BookingDetails.Add(newBookingDetail);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return await GetBookingByIdAsync(newBooking.Id) ?? throw new Exception("Failed to retrieve created booking details.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BookingResponseModel?> RejectBookingAsync(long id)
        {
            var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
            if (existingBooking == null) return null;

            existingBooking.BookingStatus = BookingStatusCodes.Rejected;
            existingBooking.UpdatedAt = DateTime.UtcNow;
            _context.Bookings.Update(existingBooking);
            await _context.SaveChangesAsync();

            return await GetBookingByIdAsync(id);
        }

        public async Task<BookingResponseModel?> ConfirmBookingAsync(long id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
                if (existingBooking == null) return null;

                existingBooking.BookingStatus = BookingStatusCodes.Confirmed;
                existingBooking.UpdatedAt = DateTime.UtcNow;
                _context.Bookings.Update(existingBooking);

                // Update linked User AccountStatus to "Active"
                if (existingBooking.UserId.HasValue)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == existingBooking.UserId.Value);
                    if (user != null)
                    {
                        user.AccountStatus = "Active";
                        user.UpdatedAt = DateTime.UtcNow;
                        _context.Users.Update(user);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetBookingByIdAsync(id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<BookingResponseModel>> MapBookingsAsync(List<DotNet8.EasyTripBackendApi.DbService.Models.Booking> bookings)
        {
            if (bookings == null || !bookings.Any())
                return new List<BookingResponseModel>();

            // Collect all item IDs for bulk retrieval
            var busIds = bookings.SelectMany(b => b.BookingDetails.Select(d => d.BusId)).Concat(bookings.Where(b => b.BookingType.Equals("Bus", StringComparison.OrdinalIgnoreCase)).Select(b => b.ItemId)).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var hotelRoomIds = bookings.SelectMany(b => b.BookingDetails.Select(d => d.HotelRoomId)).Concat(bookings.Where(b => b.BookingType.Equals("Hotel", StringComparison.OrdinalIgnoreCase)).Select(b => b.ItemId)).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var packageIds = bookings.SelectMany(b => b.BookingDetails.Select(d => d.PackageId)).Concat(bookings.Where(b => b.BookingType.Equals("Package", StringComparison.OrdinalIgnoreCase) || b.BookingType.Equals("TravelPackage", StringComparison.OrdinalIgnoreCase)).Select(b => b.ItemId)).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

            var buses = busIds.Any()
                ? await _context.Buses.Where(b => busIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b)
                : new Dictionary<long, DotNet8.EasyTripBackendApi.DbService.Models.Bus>();

            var hotelRooms = hotelRoomIds.Any()
                ? await _context.HotelRooms
                    .Include(hr => hr.Hotel)
                    .Include(hr => hr.RoomType)
                    .Where(hr => hotelRoomIds.Contains(hr.Id))
                    .ToDictionaryAsync(hr => hr.Id, hr => hr)
                : new Dictionary<long, HotelRoom>();

            var packages = packageIds.Any()
                ? await _context.TravelPackages.Where(tp => packageIds.Contains(tp.Id)).ToDictionaryAsync(tp => tp.Id, tp => tp)
                : new Dictionary<long, TravelPackage>();

            return bookings.Select(b => {
                var detail = b.BookingDetails.FirstOrDefault();

                long? busId = detail?.BusId ?? (b.BookingType.Equals("Bus", StringComparison.OrdinalIgnoreCase) ? b.ItemId : null);
                long? hotelRoomId = detail?.HotelRoomId ?? (b.BookingType.Equals("Hotel", StringComparison.OrdinalIgnoreCase) ? b.ItemId : null);
                long? packageId = detail?.PackageId ?? (b.BookingType.Equals("Package", StringComparison.OrdinalIgnoreCase) || b.BookingType.Equals("TravelPackage", StringComparison.OrdinalIgnoreCase) ? b.ItemId : null);

                DotNet8.EasyTripBackendApi.DbService.Models.Bus? bus = null;
                if (busId.HasValue) buses.TryGetValue(busId.Value, out bus);

                HotelRoom? room = null;
                if (hotelRoomId.HasValue) hotelRooms.TryGetValue(hotelRoomId.Value, out room);

                TravelPackage? package = null;
                if (packageId.HasValue) packages.TryGetValue(packageId.Value, out package);

                string? itemName = null;
                long? resolvedItemId = null;

                if (b.BookingType.Equals("Bus", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = bus?.BusName;
                    resolvedItemId = busId;
                }
                else if (b.BookingType.Equals("Hotel", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = room?.RoomType != null && room?.Hotel != null 
                        ? $"{room.RoomType.TypeName} at {room.Hotel.HotelName}" 
                        : "Hotel Room";
                    resolvedItemId = hotelRoomId;
                }
                else if (b.BookingType.Equals("Package", StringComparison.OrdinalIgnoreCase) || 
                         b.BookingType.Equals("TravelPackage", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = package?.PackageName;
                    resolvedItemId = packageId;
                }

                BookingDetailResponseModel? detailModel = null;
                if (detail != null)
                {
                    detailModel = new BookingDetailResponseModel
                    {
                        Id = detail.Id,
                        BusId = detail.BusId,
                        BusName = bus?.BusName,
                        BusNumber = bus?.BusNumber,
                        HotelRoomId = detail.HotelRoomId,
                        HotelRoomTypeName = room?.RoomType?.TypeName,
                        HotelName = room?.Hotel?.HotelName,
                        PackageId = detail.PackageId,
                        PackageName = package?.PackageName,
                        SelectedSeats = detail.SelectedSeats,
                        Quantity = detail.Quantity,
                        TravelDate = detail.TravelDate,
                        EndDate = detail.EndDate
                    };
                }
                else
                {
                    detailModel = new BookingDetailResponseModel
                    {
                        Id = 0,
                        BusId = busId,
                        BusName = bus?.BusName,
                        BusNumber = bus?.BusNumber,
                        HotelRoomId = hotelRoomId,
                        HotelRoomTypeName = room?.RoomType?.TypeName,
                        HotelName = room?.Hotel?.HotelName,
                        PackageId = packageId,
                        PackageName = package?.PackageName,
                        SelectedSeats = null,
                        Quantity = 1,
                        TravelDate = b.TravelDate,
                        EndDate = null
                    };
                }

                return new BookingResponseModel
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    UserName = b.User?.Name,
                    UserEmail = b.User?.Email,
                    BookingType = b.BookingType,
                    ItemId = resolvedItemId,
                    ItemName = itemName,
                    BookingDate = b.BookingDate,
                    TravelDate = b.TravelDate,
                    TotalAmount = b.TotalAmount,
                    PaymentStatus = (PaymentStatus)(b.PaymentStatus ?? 0),
                    BookingStatus = (BookingStatus)(b.BookingStatus ?? 0),
                    BookingStatusCode = b.BookingStatus ?? 0,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    DeletedAt = b.DeletedAt,
                    Details = detailModel
                };
            }).ToList();
        }
    }
}
