using System;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Reports
{
    public interface IReportService
    {
        Task<SalesRevenueReportModel> GetSalesRevenueReportAsync(DateTime? startDate, DateTime? endDate);
        Task<BookingAnalyticsReportModel> GetBookingAnalyticsReportAsync(DateTime? startDate, DateTime? endDate);
        Task<TopServicesReportModel> GetTopServicesReportAsync(DateTime? startDate, DateTime? endDate, string? serviceType, int top, string? metric);
    }
}
