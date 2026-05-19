using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.RoomTypes
{
    public interface IRoomTypeService
    {
        Task<List<RoomTypeResponseModel>> GetRoomTypesAsync();
    }
}
