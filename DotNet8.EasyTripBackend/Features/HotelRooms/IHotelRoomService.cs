using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.HotelRooms
{
    public interface IHotelRoomService
    {
        Task<List<HotelRoomResponseModel>> GetHotelRoomsByHotelAsync(long hotelId);
        Task<HotelRoomResponseModel?> GetHotelRoomAsync(long id);
        Task<HotelRoomResponseModel> CreateHotelRoomAsync(long hotelId, HotelRoomRequestModel request);
        Task<HotelRoomResponseModel?> UpdateHotelRoomAsync(long id, HotelRoomRequestModel request);
        Task<bool> DeleteHotelRoomAsync(long id);
    }
}
