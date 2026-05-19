using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.TravelPackages
{
    public interface ITravelPackageService
    {
        Task<PaginationResponse<TravelPackageResponseModel>> GetTravelPackagesAsync(int pageNo, int pageSize);
        Task<TravelPackageResponseModel?> GetTravelPackageAsync(long id);
        Task<TravelPackageResponseModel> CreateTravelPackageAsync(TravelPackageRequestModel request);
        Task<TravelPackageResponseModel?> UpdateTravelPackageAsync(long id, TravelPackageRequestModel request);
        Task<bool> DeleteTravelPackageAsync(long id);
    }
}
