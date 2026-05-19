using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.HotelRooms
{
    public class HotelRoomService : IHotelRoomService
    {
        private readonly AppDbContext _context;

        public HotelRoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<HotelRoomResponseModel>> GetHotelRoomsByHotelAsync(long hotelId)
        {
            return await _context.HotelRooms
                .Where(hr => hr.HotelId == hotelId && hr.DeletedAt == null)
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
                })
                .ToListAsync();
        }

        public async Task<HotelRoomResponseModel?> GetHotelRoomAsync(long id)
        {
            return await _context.HotelRooms
                .Where(hr => hr.Id == id && hr.DeletedAt == null)
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
                })
                .FirstOrDefaultAsync();
        }

        public async Task<HotelRoomResponseModel> CreateHotelRoomAsync(long hotelId, HotelRoomRequestModel request)
        {
            var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == hotelId && h.DeletedAt == null);
            if (!hotelExists) throw new Exception("Hotel not found");

            if (request.RoomTypeId.HasValue)
            {
                var typeExists = await _context.RoomTypes.AnyAsync(rt => rt.Id == request.RoomTypeId.Value);
                if (!typeExists) throw new Exception("Room type not found");
            }

            var newRoom = new HotelRoom
            {
                HotelId = hotelId,
                RoomTypeId = request.RoomTypeId,
                PricePerNight = request.PricePerNight,
                Amenities = request.Amenities,
                AvailableRooms = request.AvailableRooms,
                HotelStatus = (int)request.HotelStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.HotelRooms.Add(newRoom);
            await _context.SaveChangesAsync();

            return await GetHotelRoomAsync(newRoom.Id) ?? throw new Exception("Failed to create hotel room");
        }

        public async Task<HotelRoomResponseModel?> UpdateHotelRoomAsync(long id, HotelRoomRequestModel request)
        {
            var existingRoom = await _context.HotelRooms.FirstOrDefaultAsync(hr => hr.Id == id && hr.DeletedAt == null);
            if (existingRoom == null) return null;

            if (request.RoomTypeId.HasValue)
            {
                var typeExists = await _context.RoomTypes.AnyAsync(rt => rt.Id == request.RoomTypeId.Value);
                if (!typeExists) throw new Exception("Room type not found");
                existingRoom.RoomTypeId = request.RoomTypeId.Value;
            }

            existingRoom.PricePerNight = request.PricePerNight;
            existingRoom.Amenities = request.Amenities ?? existingRoom.Amenities;
            existingRoom.AvailableRooms = request.AvailableRooms;
            existingRoom.HotelStatus = (int)request.HotelStatus;

            _context.HotelRooms.Update(existingRoom);
            await _context.SaveChangesAsync();

            return await GetHotelRoomAsync(id);
        }

        public async Task<bool> DeleteHotelRoomAsync(long id)
        {
            var existingRoom = await _context.HotelRooms.FirstOrDefaultAsync(hr => hr.Id == id && hr.DeletedAt == null);
            if (existingRoom == null) return false;

            // Soft delete
            existingRoom.DeletedAt = DateTime.UtcNow;

            _context.HotelRooms.Update(existingRoom);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
