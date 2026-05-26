using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using DotNet8.EasyTrip.App.Client.Services;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.Reports
{
    public partial class TopServicesReport
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ReportExportService Exporter { get; set; } = null!;

        private bool _isLoading = true;
        private string? _errorMessage;
        private TopServicesReportModel? _report;
        private DateTime? _startDate;
        private DateTime? _endDate = DateTime.Today;
        private string _serviceType = "All";
        private string _metric = "revenue";
        private int _topCount = 10;

        protected override async Task OnInitializedAsync()
        {
            _startDate = DateTime.Today.AddDays(-29);
            await LoadReportAsync();
        }

        private async Task LoadReportAsync()
        {
            _isLoading = true;
            StateHasChanged();
            try
            {
                var url = $"api/report/top-service?type={Uri.EscapeDataString(_serviceType)}&top={_topCount}&metric={Uri.EscapeDataString(_metric)}";
                if (_startDate.HasValue)
                    url += $"&startDate={_startDate.Value:yyyy-MM-dd}";
                if (_endDate.HasValue)
                    url += $"&endDate={_endDate.Value:yyyy-MM-dd}";

                var response = await Http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _errorMessage = await response.Content.ReadAsStringAsync();
                    _report = null;
                    return;
                }

                _report = await response.Content.ReadFromJsonAsync<TopServicesReportModel>();
                _errorMessage = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TopServicesReport: " + ex.Message);
                _errorMessage = ex.Message;
                _report = null;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private decimal CombinedRevenue => _report?.Services.Sum(s => s.TotalRevenue) ?? 0;
        private int TotalBookingsCount => _report?.Services.Sum(s => s.BookingCount) ?? 0;

        private static string RankClass(int rank) => rank switch
        {
            1 => "rpt-rank rpt-rank--1",
            2 => "rpt-rank rpt-rank--2",
            3 => "rpt-rank rpt-rank--3",
            _ => "rpt-rank"
        };

        private async Task ExportExcelAsync()
        {
            if (_report == null) return;

            await Exporter.DownloadExcelAsync(
                "Top Performing Services Report",
                $"top-services-{_report.StartDate:yyyyMMdd}-{_report.EndDate:yyyyMMdd}",
                BuildExportRows());
        }

        private async Task ExportPdfAsync()
        {
            if (_report == null) return;

            await Exporter.DownloadPdfAsync(
                "Top Performing Services Report",
                $"top-services-{_report.StartDate:yyyyMMdd}-{_report.EndDate:yyyyMMdd}",
                BuildSummaryLines(),
                BuildExportRows());
        }

        private IEnumerable<string> BuildSummaryLines()
        {
            if (_report == null) yield break;

            yield return $"Period: {_report.StartDate:dd-MM-yyyy} to {_report.EndDate:dd-MM-yyyy}";
            yield return $"Service Type: {_report.ServiceTypeFilter}";
            yield return $"Rank By: {_report.Metric}";
            yield return $"Combined Revenue: {ReportExportService.Money(CombinedRevenue)}";
            yield return $"Total Bookings: {TotalBookingsCount}";
        }

        private IEnumerable<string[]> BuildExportRows()
        {
            if (_report == null) yield break;

            yield return new[] { "Rank", "Service", "Type", "Bookings", "Revenue", "Avg Order", "Description" };
            foreach (var item in _report.Services)
            {
                yield return new[]
                {
                    item.Rank.ToString(),
                    item.ServiceName,
                    item.ServiceType,
                    item.BookingCount.ToString(),
                    ReportExportService.Money(item.TotalRevenue),
                    ReportExportService.Money(item.AverageOrderValue),
                    item.Description ?? string.Empty
                };
            }
        }
    }
}
