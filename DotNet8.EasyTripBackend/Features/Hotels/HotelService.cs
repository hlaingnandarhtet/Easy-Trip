using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Hotels
{
    public class HotelService : IHotelService
    {
        private readonly AppDbContext _context;

        public HotelService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResponse<HotelResponseModel>> GetHotelsAsync(int pageNo, int pageSize, string? search = null)
        {
            var query = _context.Hotels
                .Where(h => h.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h => h.HotelName.Contains(search) || h.Location.Contains(search));
            }

            var projectedQuery = query.Select(h => new HotelResponseModel
            {
                Id = h.Id,
                HotelName = h.HotelName,
                Location = h.Location,
                CreatedAt = h.CreatedAt,
                DeletedAt = h.DeletedAt,
                HotelRooms = h.HotelRooms
                    .Where(hr => hr.DeletedAt == null)
                    .Select(hr => new HotelRoomResponseModel
                    {
                        Id = hr.Id,
                        HotelId = hr.HotelId,
                        RoomTypeId = hr.RoomTypeId,
                        RoomTypeName = hr.RoomType != null ? hr.RoomType.TypeName : null,
                        PricePerNight = hr.PricePerNight,
                        Amenities = hr.Amenities,
                        AvailableRooms = hr.AvailableRooms,
                        HotelStatus = (HotelStatus)(hr.HotelStatus ?? 0),
                        CreatedAt = hr.CreatedAt,
                        DeletedAt = hr.DeletedAt
                    }).ToList()
            })
            .OrderByDescending(h => h.Id);

            return await projectedQuery.ToPagedListAsync(pageNo, pageSize);
        }

        public async Task<HotelResponseModel?> GetHotelById(long id)
        {
            var hotel = await _context.Hotels
                .Where(h => h.Id == id && h.DeletedAt == null)
                .Select(h => new HotelResponseModel
                {
                    Id = h.Id,
                    HotelName = h.HotelName,
                    Location = h.Location,
                    CreatedAt = h.CreatedAt,
                    DeletedAt = h.DeletedAt,
                    HotelRooms = h.HotelRooms
                        .Where(hr => hr.DeletedAt == null)
                        .Select(hr => new HotelRoomResponseModel
                        {
                            Id = hr.Id,
                            HotelId = hr.HotelId,
                            RoomTypeId = hr.RoomTypeId,
                            RoomTypeName = hr.RoomType != null ? hr.RoomType.TypeName : null,
                            PricePerNight = hr.PricePerNight,
                            Amenities = hr.Amenities,
                            AvailableRooms = hr.AvailableRooms,
                            HotelStatus = (HotelStatus)(hr.HotelStatus ?? 0),
                            CreatedAt = hr.CreatedAt,
                            DeletedAt = hr.DeletedAt
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return hotel;
        }

        public async Task<HotelResponseModel> CreateHotelAsync(HotelRequestModel request)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var newHotel = new Hotel
                    {
                        HotelName = request.HotelName ?? "",
                        Location = request.Location ?? "",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Hotels.Add(newHotel);
                    await _context.SaveChangesAsync();

                    if (request.HotelRooms != null && request.HotelRooms.Any())
                    {
                        foreach (var roomReq in request.HotelRooms)
                        {
                            var room = new HotelRoom
                            {
                                HotelId = newHotel.Id,
                                RoomTypeId = roomReq.RoomTypeId,
                                PricePerNight = roomReq.PricePerNight,
                                Amenities = roomReq.Amenities,
                                AvailableRooms = roomReq.AvailableRooms,
                                HotelStatus = (int)roomReq.HotelStatus,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.HotelRooms.Add(room);
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return await GetHotelById(newHotel.Id) ?? throw new Exception("Failed to create hotel");
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<HotelResponseModel?> UpdateHotelAsync(long id, HotelRequestModel request)
        {
            var existingHotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id && h.DeletedAt == null);
            if (existingHotel == null) return null;

            existingHotel.HotelName = request.HotelName ?? existingHotel.HotelName;
            existingHotel.Location = request.Location ?? existingHotel.Location;

            _context.Hotels.Update(existingHotel);
            await _context.SaveChangesAsync();

            return await GetHotelById(id);
        }

        public async Task<bool> DeleteHotelAsync(long id)
        {
            var existingHotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id && h.DeletedAt == null);
            if (existingHotel == null) return false;

            // Soft delete
            existingHotel.DeletedAt = DateTime.UtcNow;

            _context.Hotels.Update(existingHotel);
            await _context.SaveChangesAsync();

            return true;
        }

        public Task<HotelResponseModel?> GetHotelAsync(long id)
        {
            return GetHotelById(id);
        }
    }
}
