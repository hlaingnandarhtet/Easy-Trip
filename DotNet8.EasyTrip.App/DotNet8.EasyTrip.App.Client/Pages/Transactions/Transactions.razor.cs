using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTrip.App.Client.Pages.Transactions
{
    public partial class Transactions
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        private MudTable<BookingResponseModel>? _table;
        private List<BookingResponseModel> _transactionsList = new();
        private int _currentPage;
        private int _pageSize;
        private int _totalUnpaidCount;
        private long? _expandedId;

        private string _searchName = "";
        private string? _selectedType;
        private DateTime? _startDate;
        private DateTime? _endDate;

        private string _chartMode = "type";
        private List<ChartSeries<double>> _chartSeries = new();
        private string[] _chartLabels = Array.Empty<string>();
        private readonly ChartOptions _chartOptions = new()
        {
            ShowLegend = true,
            ChartPalette = new[] { "#14B8A6", "#F9A8D4", "#94A3B8" }
        };

        private int GetRowNumber(BookingResponseModel context)
        {
            var index = _transactionsList.IndexOf(context);
            if (index < 0) return 0;
            return (_currentPage * _pageSize) + index + 1;
        }

        private static string GetTransactionId(BookingResponseModel booking) => $"TXN-{booking.Id:D5}";

        private static string GetTransactionDate(BookingResponseModel booking)
        {
            var date = booking.CreatedAt ?? booking.BookingDate;
            return date.HasValue ? date.Value.ToLocalTime().ToString("dd-MM-yyyy HH:mm") : "—";
        }

        private static string GetServiceDescription(BookingResponseModel b)
        {
            if (b.Details == null) return b.ItemName ?? "—";
            return b.BookingType switch
            {
                "Bus" => $"{b.Details.BusName ?? b.Details.BusNumber ?? "—"}" +
                         (string.IsNullOrEmpty(b.Details.SelectedSeats) ? "" : $" (Seats: {b.Details.SelectedSeats})"),
                "Hotel" => $"{b.Details.HotelName ?? "—"} — {b.Details.HotelRoomTypeName ?? "Room"} × {b.Details.Quantity}",
                _ => $"{b.Details.PackageName ?? b.ItemName ?? "—"}"
            };
        }

        private void ToggleExpand(BookingResponseModel booking)
        {
            _expandedId = _expandedId == booking.Id ? null : booking.Id;
            StateHasChanged();
        }

        private void PerformSearch() => _table?.ReloadServerData();

        private void ResetFilters()
        {
            _searchName = "";
            _selectedType = null;
            _startDate = null;
            _endDate = null;
            _table?.ReloadServerData();
        }

        private async Task<TableData<BookingResponseModel>> LoadServerData(TableState state, CancellationToken token)
        {
            try
            {
                var pageNo = state.Page + 1;
                var pageSize = state.PageSize;
                var url = $"api/booking?pageNo={pageNo}&pageSize={pageSize}&newestFirst=true&filterByCreatedDate=true&paymentStatus={(int)PaymentStatus.Unpaid}";

                if (!string.IsNullOrWhiteSpace(_searchName))
                    url += $"&name={Uri.EscapeDataString(_searchName)}";
                if (!string.IsNullOrWhiteSpace(_selectedType))
                    url += $"&type={Uri.EscapeDataString(_selectedType)}";
                if (_startDate.HasValue)
                    url += $"&startDate={DateOnly.FromDateTime(_startDate.Value):yyyy-MM-dd}";
                if (_endDate.HasValue)
                    url += $"&endDate={DateOnly.FromDateTime(_endDate.Value):yyyy-MM-dd}";

                var response = await Http.GetFromJsonAsync<PaginationResponse<BookingResponseModel>>(url, token);
                if (response != null)
                {
                    _transactionsList = response.Data.ToList();
                    _totalUnpaidCount = response.TotalCount;
                    _currentPage = state.Page;
                    _pageSize = state.PageSize;
                    UpdateChart();
                    return new TableData<BookingResponseModel>
                    {
                        TotalItems = response.TotalCount,
                        Items = response.Data
                    };
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading transactions: {ex.Message}", Severity.Error);
            }

            return new TableData<BookingResponseModel> { TotalItems = 0, Items = Array.Empty<BookingResponseModel>() };
        }

        private void UpdateChart()
        {
            _chartLabels = new[] { "Bus", "Hotel", "Package" };
            if (_chartMode == "amount")
            {
                _chartSeries = new List<ChartSeries<double>>
                {
                    new() { Name = "Pending", Data = TypeAmountByStatus(BookingStatusCodes.Pending) },
                    new() { Name = "Confirmed", Data = TypeAmountByStatus(BookingStatusCodes.Confirmed) },
                    new() { Name = "Rejected", Data = TypeAmountByStatus(BookingStatusCodes.Rejected) }
                };
            }
            else
            {
                _chartSeries = new List<ChartSeries<double>>
                {
                    new() { Name = "Pending", Data = TypeCountByStatus(BookingStatusCodes.Pending) },
                    new() { Name = "Confirmed", Data = TypeCountByStatus(BookingStatusCodes.Confirmed) },
                    new() { Name = "Rejected", Data = TypeCountByStatus(BookingStatusCodes.Rejected) }
                };
            }
        }

        private double[] TypeCountByStatus(int status) => new double[]
        {
            _transactionsList.Count(x => x.BookingType == "Bus" && x.BookingStatusCode == status),
            _transactionsList.Count(x => x.BookingType == "Hotel" && x.BookingStatusCode == status),
            _transactionsList.Count(x => x.BookingType is "Package" or "TravelPackage" && x.BookingStatusCode == status)
        };

        private double[] TypeAmountByStatus(int status) => new[]
        {
            (double)_transactionsList.Where(x => x.BookingType == "Bus" && x.BookingStatusCode == status).Sum(x => x.TotalAmount),
            (double)_transactionsList.Where(x => x.BookingType == "Hotel" && x.BookingStatusCode == status).Sum(x => x.TotalAmount),
            (double)_transactionsList.Where(x => x.BookingType is "Package" or "TravelPackage" && x.BookingStatusCode == status).Sum(x => x.TotalAmount)
        };

        private static RenderFragment RenderBookingBadge(int code) => builder =>
        {
            var (text, css) = code switch
            {
                BookingStatusCodes.Pending => ("Pending", "et-badge et-badge--warning"),
                BookingStatusCodes.Confirmed => ("Confirmed", "et-badge et-badge--success"),
                BookingStatusCodes.Rejected => ("Rejected", "et-badge et-badge--danger"),
                _ => ("Unknown", "et-badge et-badge--muted")
            };
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", css);
            builder.AddContent(2, text);
            builder.CloseElement();
        };
    }
}
