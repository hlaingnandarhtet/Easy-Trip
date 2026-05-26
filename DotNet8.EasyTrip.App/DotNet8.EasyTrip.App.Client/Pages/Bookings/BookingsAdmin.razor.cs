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

namespace DotNet8.EasyTrip.App.Client.Pages.Bookings
{
    public partial class BookingsAdmin
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        [SupplyParameterFromQuery(Name = "status")]
        public int? StatusQuery { get; set; }

        private MudTable<BookingResponseModel>? _table;
        private long? _processingId;
        private long? _expandedId;
        private List<BookingResponseModel> _bookingsList = new();
        private int _currentPage;
        private int _pageSize;
        private bool _queryApplied;

        private string _searchName = "";
        private string? _selectedType;
        private int? _selectedStatus;
        private DateTime? _startDate;
        private DateTime? _endDate;

        protected override void OnParametersSet()
        {
            if (StatusQuery.HasValue)
                _selectedStatus = StatusQuery;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && StatusQuery.HasValue && !_queryApplied)
            {
                _queryApplied = true;
                await (_table?.ReloadServerData() ?? Task.CompletedTask);
            }
        }

        private int GetRowNumber(BookingResponseModel context)
        {
            var index = _bookingsList.IndexOf(context);
            if (index < 0) return 0;
            return (_currentPage * _pageSize) + index + 1;
        }

        private void ToggleExpand(BookingResponseModel booking)
        {
            _expandedId = _expandedId == booking.Id ? null : booking.Id;
            StateHasChanged();
        }

        private static RenderFragment RenderPaymentBadge(PaymentStatus status) => builder =>
        {
            var (text, css) = status switch
            {
                PaymentStatus.Paid => ("Paid", "et-badge et-badge--success"),
                PaymentStatus.UnderReview => ("Under Review", "et-badge et-badge--warning"),
                _ => ("Unpaid", "et-badge et-badge--muted")
            };
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", css);
            builder.AddContent(2, text);
            builder.CloseElement();
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

        private void PerformSearch() => _table?.ReloadServerData();

        private void ResetFilters()
        {
            _searchName = "";
            _selectedType = null;
            _selectedStatus = null;
            _startDate = null;
            _endDate = null;
            _queryApplied = false;
            _table?.ReloadServerData();
        }

        private async Task<TableData<BookingResponseModel>> LoadServerData(TableState state, CancellationToken token)
        {
            try
            {
                var pageNo = state.Page + 1;
                var pageSize = state.PageSize;
                var url = $"api/booking?pageNo={pageNo}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(_searchName))
                    url += $"&name={Uri.EscapeDataString(_searchName)}";
                if (!string.IsNullOrWhiteSpace(_selectedType))
                    url += $"&type={Uri.EscapeDataString(_selectedType)}";
                if (_selectedStatus.HasValue)
                    url += $"&status={_selectedStatus.Value}";
                if (_startDate.HasValue)
                    url += $"&startDate={_startDate.Value:yyyy-MM-dd}";
                if (_endDate.HasValue)
                    url += $"&endDate={_endDate.Value:yyyy-MM-dd}";

                var response = await Http.GetFromJsonAsync<PaginationResponse<BookingResponseModel>>(url);
                if (response != null)
                {
                    _bookingsList = response.Data.ToList();
                    _currentPage = state.Page;
                    _pageSize = state.PageSize;
                    return new TableData<BookingResponseModel>
                    {
                        TotalItems = response.TotalCount,
                        Items = response.Data
                    };
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading bookings: {ex.Message}", Severity.Error);
            }

            return new TableData<BookingResponseModel> { TotalItems = 0, Items = Array.Empty<BookingResponseModel>() };
        }

        private async Task ConfirmBooking(long id)
        {
            _processingId = id;
            try
            {
                var response = await Http.PostAsync($"api/admin/booking/confirm/{id}", null);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Booking confirmed.", Severity.Success);
                    _table?.ReloadServerData();
                }
                else
                    Snackbar.Add(await response.Content.ReadAsStringAsync(), Severity.Error);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                _processingId = null;
            }
        }

        private async Task RejectBooking(long id)
        {
            _processingId = id;
            try
            {
                var response = await Http.PostAsync($"api/admin/booking/reject/{id}", null);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Booking rejected. Bus seats are available again.", Severity.Info);
                    _table?.ReloadServerData();
                }
                else
                    Snackbar.Add(await response.Content.ReadAsStringAsync(), Severity.Error);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                _processingId = null;
            }
        }
    }
}
