using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTrip.App.Client.Services;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.Reports
{
    public partial class BookingAnalyticsReport
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ReportExportService Exporter { get; set; } = null!;

        private bool _isLoading = true;
        private string? _errorMessage;
        private BookingAnalyticsReportModel? _report;
        private DateTime? _startDate;
        private DateTime? _endDate = DateTime.Today;

        private List<ChartSeries<double>> _volumeSeries = new();
        private List<ChartSeries<double>> _statusDonut = new();
        private string[] _chartLabels = Array.Empty<string>();
        private readonly ChartOptions _lineOptions = new()
        {
            ShowLegend = true,
            ChartPalette = new[] { "#6366F1", "#10B981", "#F59E0B" }
        };
        private readonly ChartOptions _donutOptions = new()
        {
            ShowLegend = true,
            ChartPalette = new[] { "#10B981", "#F59E0B", "#EF4444" }
        };

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
                var url = "api/report/booking-analytic";
                if (_startDate.HasValue)
                    url += $"?startDate={_startDate.Value:yyyy-MM-dd}";
                if (_endDate.HasValue)
                    url += (_startDate.HasValue ? "&" : "?") + $"endDate={_endDate.Value:yyyy-MM-dd}";

                var response = await Http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _errorMessage = await response.Content.ReadAsStringAsync();
                    _report = null;
                    return;
                }

                _report = await response.Content.ReadFromJsonAsync<BookingAnalyticsReportModel>();
                _errorMessage = null;
                UpdateCharts();
            }
            catch (Exception ex)
            {
                Console.WriteLine("BookingAnalyticsReport: " + ex.Message);
                _errorMessage = ex.Message;
                _report = null;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void UpdateCharts()
        {
            if (_report == null) return;

            var rawLabels = _report.ChartLabels.ToArray();
            _chartLabels = ThinLabels(rawLabels, stepSize: 5);
            _volumeSeries = new List<ChartSeries<double>>
            {
                new() { Name = "All Bookings", Data = _report.BookingVolumeSeries.ToArray() },
                new() { Name = "Confirmed", Data = _report.ConfirmedVolumeSeries.ToArray() },
                new() { Name = "Pending", Data = _report.PendingVolumeSeries.ToArray() }
            };

            _statusDonut = new List<ChartSeries<double>>
            {
                new() { Name = "Confirmed", Data = new[] { (double)Math.Max(0, _report.ConfirmedCount) } },
                new() { Name = "Pending", Data = new[] { (double)Math.Max(0, _report.PendingCount) } },
                new() { Name = "Rejected", Data = new[] { (double)Math.Max(0, _report.RejectedCount) } }
            };
        }

        /// <summary>
        /// Returns a label array where only every <paramref name="stepSize"/>-th entry is kept;
        /// the rest are replaced with an empty string so the chart still renders all data
        /// points but only shows legible x-axis ticks.
        /// </summary>
        private static string[] ThinLabels(string[] labels, int stepSize)
        {
            if (labels.Length <= stepSize) return labels;
            var result = new string[labels.Length];
            for (int i = 0; i < labels.Length; i++)
                result[i] = (i % stepSize == 0) ? labels[i] : string.Empty;
            return result;
        }

        private async Task ExportExcelAsync()
        {
            if (_report == null) return;

            await Exporter.DownloadExcelAsync(
                "Booking Analytics Report",
                $"booking-analytics-{_report.StartDate:yyyyMMdd}-{_report.EndDate:yyyyMMdd}",
                BuildExportRows());
        }

        private async Task ExportPdfAsync()
        {
            if (_report == null) return;

            await Exporter.DownloadPdfAsync(
                "Booking Analytics Report",
                $"booking-analytics-{_report.StartDate:yyyyMMdd}-{_report.EndDate:yyyyMMdd}",
                BuildSummaryLines(),
                BuildExportRows());
        }

        private IEnumerable<string> BuildSummaryLines()
        {
            if (_report == null) yield break;

            yield return $"Period: {_report.StartDate:dd-MM-yyyy} to {_report.EndDate:dd-MM-yyyy}";
            yield return $"Total Bookings: {_report.TotalBookings}";
            yield return $"Conversion Rate: {_report.ConversionRate:F1}%";
            yield return $"Average Booking Value: {ReportExportService.Money(_report.AverageBookingAmount)}";
            yield return $"Total Booking Value: {ReportExportService.Money(_report.TotalBookingValue)}";
        }

        private IEnumerable<string[]> BuildExportRows()
        {
            if (_report == null) yield break;

            yield return new[] { "Metric", "Value" };
            yield return new[] { "Period", $"{_report.StartDate:dd-MM-yyyy} to {_report.EndDate:dd-MM-yyyy}" };
            yield return new[] { "Total Bookings", _report.TotalBookings.ToString() };
            yield return new[] { "Confirmed", _report.ConfirmedCount.ToString() };
            yield return new[] { "Pending", _report.PendingCount.ToString() };
            yield return new[] { "Rejected", _report.RejectedCount.ToString() };
            yield return new[] { "Paid", _report.PaidCount.ToString() };
            yield return new[] { "Unpaid", _report.UnpaidCount.ToString() };
            yield return new[] { "Under Review", _report.UnderReviewCount.ToString() };
            yield return new[] { "Bus Bookings", _report.BusCount.ToString() };
            yield return new[] { "Hotel Bookings", _report.HotelCount.ToString() };
            yield return new[] { "Package Bookings", _report.PackageCount.ToString() };
            yield return new[] { "Average Booking Value", ReportExportService.Money(_report.AverageBookingAmount) };
            yield return new[] { "Total Booking Value", ReportExportService.Money(_report.TotalBookingValue) };
            yield return new[] { "Conversion Rate", $"{_report.ConversionRate:F1}%" };
        }
    }
}
