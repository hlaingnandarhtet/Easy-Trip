using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.Reports
{
    public partial class TopServicesReport
    {
        [Inject] private HttpClient Http { get; set; } = null!;

        private bool _isLoading = true;
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
                var url = $"api/reports/top-services?type={Uri.EscapeDataString(_serviceType)}&top={_topCount}&metric={Uri.EscapeDataString(_metric)}";
                if (_startDate.HasValue)
                    url += $"&startDate={_startDate.Value:yyyy-MM-dd}";
                if (_endDate.HasValue)
                    url += $"&endDate={_endDate.Value:yyyy-MM-dd}";

                _report = await Http.GetFromJsonAsync<TopServicesReportModel>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TopServicesReport: " + ex.Message);
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
    }
}
