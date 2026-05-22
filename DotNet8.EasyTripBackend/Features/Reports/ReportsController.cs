using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8.EasyTripBackend.Features.Reports
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("sales-revenue")]
        public async Task<IActionResult> GetSalesRevenue(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GetSalesRevenueReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating sales revenue report: {ex.Message}");
            }
        }

        [HttpGet("booking-analytics")]
        public async Task<IActionResult> GetBookingAnalytics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GetBookingAnalyticsReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating booking analytics report: {ex.Message}");
            }
        }

        [HttpGet("top-services")]
        public async Task<IActionResult> GetTopServices(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? type = "All",
            [FromQuery] int top = 10,
            [FromQuery] string? metric = "revenue")
        {
            try
            {
                var report = await _reportService.GetTopServicesReportAsync(startDate, endDate, type, top, metric);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating top services report: {ex.Message}");
            }
        }
    }
}
