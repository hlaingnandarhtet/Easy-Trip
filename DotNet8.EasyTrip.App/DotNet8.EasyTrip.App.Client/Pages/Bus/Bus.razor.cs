using System.Threading;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTrip.App.Client.Pages.Bus
{
    public partial class Bus
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        private MudTable<BusResponseModel>? _table;
        private string _searchString = "";
        private string _searchQuery = "";

        private List<BusResponseModel> _busesList = new();
        private int _totalCount;
        private int _currentPage;
        private int _pageSize;

        private int GetRowNumber(BusResponseModel context)
        {
            var index = _busesList.IndexOf(context);
            if (index < 0) return 0;
            return (_currentPage * _pageSize) + index + 1;
        }

        private async Task<TableData<BusResponseModel>> LoadServerData(TableState state, CancellationToken token)
        {
            try
            {
                var pageNo = state.Page + 1;
                var pageSize = state.PageSize;
                var url = $"api/Bus?pageNo={pageNo}&pageSize={pageSize}";
                if (!string.IsNullOrWhiteSpace(_searchQuery))
                {
                    url += $"&search={Uri.EscapeDataString(_searchQuery)}";
                }

                var response = await Http.GetFromJsonAsync<PaginationResponse<BusResponseModel>>(url);
                if (response != null)
                {
                    _busesList = response.Data.ToList();
                    _totalCount = response.TotalCount;
                    _currentPage = state.Page;
                    _pageSize = state.PageSize;

                    return new TableData<BusResponseModel>
                    {
                        TotalItems = response.TotalCount,
                        Items = response.Data
                    };
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading buses: {ex.Message}", Severity.Error);
            }

            return new TableData<BusResponseModel> { TotalItems = 0, Items = Array.Empty<BusResponseModel>() };
        }

        private void PerformSearch()
        {
            _searchQuery = _searchString;
            _table?.ReloadServerData();
        }

        private void ResetFilters()
        {
            _searchString = "";
            _searchQuery = "";
            _table?.ReloadServerData();
        }

        private void OpenCreateDialog()
        {
            Nav.NavigateTo("/bus/create");
        }

        private void OpenEditDialog(BusResponseModel bus)
        {
            Nav.NavigateTo($"/bus/edit/{bus.Id}");
        }

        private async Task OpenDeleteDialog(long id)
        {
            var parameters = new DialogParameters { ["BusId"] = id };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<BusDelete>("Confirm Delete", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                _table?.ReloadServerData();
            }
        }
    }
}
