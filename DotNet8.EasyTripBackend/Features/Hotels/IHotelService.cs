using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Hotels
{
    public interface IHotelService
    {
        Task<PaginationResponse<HotelResponseModel>> GetHotelsAsync(int pageNo, int pageSize, string? search = null);
        Task<HotelResponseModel?> GetHotelAsync(long id);
        Task<HotelResponseModel> CreateHotelAsync(HotelRequestModel request);
        Task<HotelResponseModel?> UpdateHotelAsync(long id, HotelRequestModel request);
        Task<bool> DeleteHotelAsync(long id);
    }
}
