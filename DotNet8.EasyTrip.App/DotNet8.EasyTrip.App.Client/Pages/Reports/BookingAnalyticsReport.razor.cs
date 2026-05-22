using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.Reports
{
    public partial class BookingAnalyticsReport
    {
        [Inject] private HttpClient Http { get; set; } = null!;

        private bool _isLoading = true;
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
                var url = "api/reports/booking-analytics";
                if (_startDate.HasValue)
                    url += $"?startDate={_startDate.Value:yyyy-MM-dd}";
                if (_endDate.HasValue)
                    url += (_startDate.HasValue ? "&" : "?") + $"endDate={_endDate.Value:yyyy-MM-dd}";

                _report = await Http.GetFromJsonAsync<BookingAnalyticsReportModel>(url);
                UpdateCharts();
            }
            catch (Exception ex)
            {
                Console.WriteLine("BookingAnalyticsReport: " + ex.Message);
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
    }
}
