using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.RoomTypes
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly AppDbContext _context;

        public RoomTypeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomTypeResponseModel>> GetRoomTypesAsync()
        {
            return await _context.RoomTypes
                .Select(rt => new RoomTypeResponseModel
                {
                    Id = rt.Id,
                    TypeName = rt.TypeName
                })
                .ToListAsync();
        }

        public async Task<RoomTypeResponseModel?> GetRoomTypeByIdAsync(long id)
        {
            var rt = await _context.RoomTypes.FindAsync(id);
            if (rt == null) return null;
            return new RoomTypeResponseModel
            {
                Id = rt.Id,
                TypeName = rt.TypeName
            };
        }

        public async Task<RoomTypeResponseModel> CreateRoomTypeAsync(RoomTypeRequestModel request)
        {
            var roomType = new RoomType
            {
                TypeName = request.TypeName ?? ""
            };
            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();
            return new RoomTypeResponseModel
            {
                Id = roomType.Id,
                TypeName = roomType.TypeName
            };
        }

        public async Task<bool> UpdateRoomTypeAsync(long id, RoomTypeRequestModel request)
        {
            var rt = await _context.RoomTypes.FindAsync(id);
            if (rt == null) return false;
            rt.TypeName = request.TypeName ?? rt.TypeName;
            _context.RoomTypes.Update(rt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoomTypeAsync(long id)
        {
            var rt = await _context.RoomTypes.FindAsync(id);
            if (rt == null) return false;
            _context.RoomTypes.Remove(rt);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
