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
    }
}
