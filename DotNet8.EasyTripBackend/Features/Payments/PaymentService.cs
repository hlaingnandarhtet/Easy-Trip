using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentMethodResponseModel>> GetPaymentMethodsAsync()
        {
            var methods = await _context.PaymentMethods
                .AsNoTracking()
                .Where(m => m.DeletedAt == null)
                .OrderByDescending(m => m.Id)
                .ToListAsync();

            return methods.Select(m => new PaymentMethodResponseModel
            {
                Id = m.Id,
                PaymentType = m.PaymentType,
                AccountName = m.AccountName,
                AccountNumber = m.AccountNumber,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                DeletedAt = m.DeletedAt
            }).ToList();
        }

        public async Task<PaymentMethodResponseModel?> GetPaymentMethodByIdAsync(long id)
        {
            var m = await _context.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);

            if (m == null) return null;

            return new PaymentMethodResponseModel
            {
                Id = m.Id,
                PaymentType = m.PaymentType,
                AccountName = m.AccountName,
                AccountNumber = m.AccountNumber,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                DeletedAt = m.DeletedAt
            };
        }

        public async Task<PaymentMethodResponseModel> CreatePaymentMethodAsync(PaymentMethodRequestModel request)
        {
            var paymentMethod = new PaymentMethod
            {
                PaymentType = request.PaymentType,
                AccountName = request.AccountName,
                AccountNumber = request.AccountNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return new PaymentMethodResponseModel
            {
                Id = paymentMethod.Id,
                PaymentType = paymentMethod.PaymentType,
                AccountName = paymentMethod.AccountName,
                AccountNumber = paymentMethod.AccountNumber,
                CreatedAt = paymentMethod.CreatedAt,
                UpdatedAt = paymentMethod.UpdatedAt
            };
        }

        public async Task<PaymentMethodResponseModel?> UpdatePaymentMethodAsync(long id, PaymentMethodRequestModel request)
        {
            var m = await _context.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);

            if (m == null) return null;

            m.PaymentType = request.PaymentType;
            m.AccountName = request.AccountName;
            m.AccountNumber = request.AccountNumber;
            m.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PaymentMethodResponseModel
            {
                Id = m.Id,
                PaymentType = m.PaymentType,
                AccountName = m.AccountName,
                AccountNumber = m.AccountNumber,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            };
        }

        public async Task<bool> DeletePaymentMethodAsync(long id)
        {
            var m = await _context.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);

            if (m == null) return false;

            m.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
