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
    public partial class SalesRevenueReport
    {
        [Inject] private HttpClient Http { get; set; } = null!;

        private bool _isLoading = true;
        private SalesRevenueReportModel? _report;
        private DateTime? _startDate;
        private DateTime? _endDate = DateTime.Today;

        private List<ChartSeries<double>> _lineSeries = new();
        private List<ChartSeries<double>> _donutSeries = new();
        private string[] _chartLabels = Array.Empty<string>();
        private readonly ChartOptions _lineOptions = new()
        {
            ShowLegend = true,
            ChartPalette = new[] { "#6366F1", "#14B8A6", "#F59E0B", "#0F172A" }
        };
        private readonly ChartOptions _donutOptions = new()
        {
            ShowLegend = true,
            ChartPalette = new[] { "#6366F1", "#14B8A6", "#F59E0B" }
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
                var url = "api/reports/sales-revenue";
                if (_startDate.HasValue)
                    url += $"?startDate={_startDate.Value:yyyy-MM-dd}";
                if (_endDate.HasValue)
                    url += (_startDate.HasValue ? "&" : "?") + $"endDate={_endDate.Value:yyyy-MM-dd}";

                _report = await Http.GetFromJsonAsync<SalesRevenueReportModel>(url);
                UpdateCharts();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SalesRevenueReport: " + ex.Message);
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
            _lineSeries = new List<ChartSeries<double>>
            {
                new() { Name = "Bus", Data = _report.BusSalesSeries.ToArray() },
                new() { Name = "Hotel", Data = _report.HotelSalesSeries.ToArray() },
                new() { Name = "Package", Data = _report.PackageSalesSeries.ToArray() }
            };

            _donutSeries = _report.RevenueByType.Select(r => new ChartSeries<double>
            {
                Name = r.Type,
                Data = new[] { (double)r.Amount }
            }).ToList();
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

        private static string FormatGrowth(double pct) =>
            pct >= 0 ? $"+{pct:F1}%" : $"{pct:F1}%";
    }
}
