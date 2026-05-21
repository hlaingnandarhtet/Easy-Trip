using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Bus
{
    public interface IBusService
    {
        Task<PaginationResponse<BusResponseModel>> GetBusesAsync(int pageNo, int pageSize, string? search = null);
        Task<BusResponseModel?> GetBusAsync(long id);
        Task<BusResponseModel> CreateBusAsync(BusRequestModel request);
        Task<BusResponseModel?> UpdateBusAsync(long id, BusRequestModel request);
        Task<bool> DeleteBusAsync(long id);
    }
}
