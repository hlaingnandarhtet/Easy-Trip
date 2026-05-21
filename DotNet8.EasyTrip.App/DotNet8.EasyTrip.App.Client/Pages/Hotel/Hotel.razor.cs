using System.Threading;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTrip.App.Client.Pages.Hotel
{
    public partial class Hotel
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        private MudTable<HotelResponseModel>? _table;
        private string _searchString = "";
        private string _searchQuery = "";

        private List<HotelResponseModel> _hotelsList = new();
        private int _totalCount;
        private int _currentPage;
        private int _pageSize;

        private int GetRowNumber(HotelResponseModel context)
        {
            var index = _hotelsList.IndexOf(context);
            if (index < 0) return 0;
            return (_currentPage * _pageSize) + index + 1;
        }

        private async Task<TableData<HotelResponseModel>> LoadServerData(TableState state, CancellationToken token)
        {
            try
            {
                var pageNo = state.Page + 1;
                var pageSize = state.PageSize;
                var url = $"api/Hotel?pageNo={pageNo}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(_searchQuery))
                {
                    url += $"&search={Uri.EscapeDataString(_searchQuery)}";
                }

                var response = await Http.GetFromJsonAsync<PaginationResponse<HotelResponseModel>>(url);
                if (response != null)
                {
                    _hotelsList = response.Data.ToList();
                    _totalCount = response.TotalCount;
                    _currentPage = state.Page;
                    _pageSize = state.PageSize;

                    return new TableData<HotelResponseModel>
                    {
                        TotalItems = response.TotalCount,
                        Items = response.Data
                    };
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading hotels: {ex.Message}", Severity.Error);
            }

            return new TableData<HotelResponseModel> { TotalItems = 0, Items = Array.Empty<HotelResponseModel>() };
        }

        private void PerformSearch()
        {
            _searchQuery = _searchString;
            _table?.ReloadServerData();
        }

        private void OpenCreateDialog()
        {
            Nav.NavigateTo("/hotel/create");
        }

        private void OpenEditDialog(HotelResponseModel hotel)
        {
            Nav.NavigateTo($"/hotel/edit/{hotel.Id}");
        }

        private async Task OpenDeleteDialog(long id)
        {
            var parameters = new DialogParameters { ["HotelId"] = id };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<HotelDelete>("Confirm Delete", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                _table?.ReloadServerData();
            }
        }
    }
}
