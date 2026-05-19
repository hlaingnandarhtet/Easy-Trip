using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTripBackend.Features.Bus
{
    public class BusService : IBusService
    {
        private readonly AppDbContext _context;

        public BusService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResponse<BusResponseModel>> GetBusesAsync(int pageNo, int pageSize)
        {
            var query = _context.Buses
                .Where(b => b.DeletedAt == null)
                .Select(b => new BusResponseModel
                {
                    Id = b.Id,
                    BusName = b.BusName,
                    BusNumber = b.BusNumber,
                    BusClass = b.BusClass,
                    TotalSeats = b.TotalSeats,
                    Price = b.Price,
                    StartPoint = b.StartPoint,
                    EndPoint = b.EndPoint,
                    Departure = b.Departure,
                    Arrival = b.Arrival,
                    DriverName = b.DriverName,
                    TripType = b.TripType,
                    TimeSlot = b.TimeSlot,
                    BusStatus = b.BusStatus,
                    CreatedAt = b.CreatedAt,
                    DeletedAt = b.DeletedAt
                })
                .OrderByDescending(b => b.Id);

            return await query.ToPagedListAsync(pageNo, pageSize);
        }

        public async Task<BusResponseModel?> GetBusById(long id)
        {
            var bus = await _context.Buses
                .Where(b => b.Id == id && b.DeletedAt == null)
                .Select(b => new BusResponseModel
                {
                    Id = b.Id,
                    BusName = b.BusName,
                    BusNumber = b.BusNumber,
                    BusClass = b.BusClass,
                    TotalSeats = b.TotalSeats,
                    Price = b.Price,
                    StartPoint = b.StartPoint,
                    EndPoint = b.EndPoint,
                    Departure = b.Departure,
                    Arrival = b.Arrival,
                    DriverName = b.DriverName,
                    TripType = b.TripType,
                    TimeSlot = b.TimeSlot,
                    BusStatus = b.BusStatus,
                    CreatedAt = b.CreatedAt,
                    DeletedAt = b.DeletedAt
                })
                .FirstOrDefaultAsync();

            return bus;
        }

        public async Task<BusResponseModel> CreateBusAsync(BusRequestModel request)
        {
            var newBus = new DotNet8.EasyTripBackendApi.DbService.Models.Bus
            {
                BusName = request.BusName ?? "",
                BusNumber = request.BusNumber ?? "",
                BusClass = request.BusClass ?? "",
                TotalSeats = request.TotalSeats ?? 0,
                Price = request.Price ?? 0,
                StartPoint = request.StartPoint ?? "",
                EndPoint = request.EndPoint ?? "",
                Departure = request.Departure ?? "",
                Arrival = request.Arrival ?? "",
                DriverName = request.DriverName ?? "",
                TripType = request.TripType ?? "",
                TimeSlot = request.TimeSlot ?? "",
                BusStatus = request.BusStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.Buses.Add(newBus);
            await _context.SaveChangesAsync();

            return await GetBusAsync(newBus.Id) ?? throw new Exception("Failed to create bus");
        }

        public async Task<BusResponseModel?> UpdateBusAsync(long id, BusRequestModel request)
        {
            var existingBus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
            if (existingBus == null) return null;

            existingBus.BusName = request.BusName ?? existingBus.BusName;
            existingBus.BusNumber = request.BusNumber ?? existingBus.BusNumber;
            existingBus.BusClass = request.BusClass ?? existingBus.BusClass;
            if (request.TotalSeats.HasValue) existingBus.TotalSeats = request.TotalSeats.Value;
            if (request.Price.HasValue) existingBus.Price = request.Price.Value;
            existingBus.StartPoint = request.StartPoint ?? existingBus.StartPoint;
            existingBus.EndPoint = request.EndPoint ?? existingBus.EndPoint;
            existingBus.Departure = request.Departure ?? existingBus.Departure;
            existingBus.Arrival = request.Arrival ?? existingBus.Arrival;
            existingBus.DriverName = request.DriverName ?? existingBus.DriverName;
            existingBus.TripType = request.TripType ?? existingBus.TripType;
            existingBus.TimeSlot = request.TimeSlot ?? existingBus.TimeSlot;
            if (request.BusStatus.HasValue) existingBus.BusStatus = request.BusStatus.Value;

            _context.Buses.Update(existingBus);
            await _context.SaveChangesAsync();

            return await GetBusAsync(id);
        }

        public async Task<bool> DeleteBusAsync(long id)
        {
            var existingBus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
            if (existingBus == null) return false;

            // Soft delete
            existingBus.DeletedAt = DateTime.UtcNow;
            
            _context.Buses.Update(existingBus);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BusResponseModel?> GetBusAsync(long id)
        {
            var bus = await _context.Buses
                                    .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);

            if (bus == null) return null;
            return new BusResponseModel
            {
                Id = bus.Id,
                BusName = bus.BusName,
                BusNumber = bus.BusNumber,
                BusClass = bus.BusClass,
                TotalSeats = bus.TotalSeats,
                Price = bus.Price,
                StartPoint = bus.StartPoint,
                EndPoint = bus.EndPoint,
                Departure = bus.Departure,
                Arrival = bus.Arrival,
                DriverName = bus.DriverName,
                TripType = bus.TripType,
                TimeSlot = bus.TimeSlot,
                BusStatus = bus.BusStatus
            };
        }
    }
}
