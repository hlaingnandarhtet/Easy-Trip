using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.BusTypes;

public interface IBusTypeService
{
    Task<List<BusTypeResponseModel>> GetBusTypesAsync();
    Task<BusTypeResponseModel?> GetBusTypeByIdAsync(long id);
    Task<BusTypeResponseModel> CreateBusTypeAsync(BusTypeRequestModel request);
    Task<bool> UpdateBusTypeAsync(long id, BusTypeRequestModel request);
    Task<bool> DeleteBusTypeAsync(long id);
}
