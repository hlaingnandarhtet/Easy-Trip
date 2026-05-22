using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Bookings
{
    public interface IBookingService
    {
        Task<PaginationResponse<BookingResponseModel>> GetBookingsAsync(
            int pageNo,
            int pageSize,
            string? name = null,
            string? type = null,
            int? status = null,
            int? paymentStatus = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            bool filterByCreatedDate = false,
            bool newestFirst = false);
        Task<BookingResponseModel?> GetBookingAsync(long id);
        Task<BookingResponseModel?> GetBookingByIdAsync(long id);
        Task<BookingResponseModel> CreateBookingAsync(BookingRequestModel request);
        Task<BookingResponseModel?> UpdateBookingAsync(long id, BookingRequestModel request);
        Task<BookingResponseModel?> UpdatePaymentAndBookingStatusAsync(long id, int paymentStatus, int bookingStatus);
        Task<bool> DeleteBookingAsync(long id);
        Task<BookingResponseModel> CreatePublicBookingAsync(PublicBookingRequestModel request);
        Task<BookingResponseModel?> ConfirmBookingAsync(long id);
        Task<BookingResponseModel?> RejectBookingAsync(long id);
        Task<List<string>> GetReservedSeatsForBusAsync(long busId, DateOnly travelDate);
        Task<List<BookingResponseModel>> GetBookingsByPhoneAsync(string phone);
    }
}

