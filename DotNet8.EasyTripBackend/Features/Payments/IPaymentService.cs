using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Payments
{
    public interface IPaymentService
    {
        Task<List<PaymentMethodResponseModel>> GetPaymentMethodsAsync();
        Task<PaymentMethodResponseModel?> GetPaymentMethodByIdAsync(long id);
        Task<PaymentMethodResponseModel> CreatePaymentMethodAsync(PaymentMethodRequestModel request);
        Task<PaymentMethodResponseModel?> UpdatePaymentMethodAsync(long id, PaymentMethodRequestModel request);
        Task<bool> DeletePaymentMethodAsync(long id);
    }
}
