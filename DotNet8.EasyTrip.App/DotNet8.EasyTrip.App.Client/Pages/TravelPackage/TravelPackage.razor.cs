using System.Threading;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTrip.App.Client.Services;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTrip.App.Client.Pages.TravelPackage
{
    public partial class TravelPackage
    {
        [Inject] private TravelPackageApiService Api { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        private MudTable<TravelPackageResponseModel>? _table;
        private string _searchString = "";
        private string _searchQuery = "";

        private List<TravelPackageResponseModel> _packagesList = new();
        private int _currentPage;
        private int _pageSize;

        private int GetRowNumber(TravelPackageResponseModel context)
        {
            var index = _packagesList.IndexOf(context);
            if (index < 0) return 0;
            return (_currentPage * _pageSize) + index + 1;
        }

        private async Task<TableData<TravelPackageResponseModel>> LoadServerData(TableState state, CancellationToken token)
        {
            try
            {
                var pageNo = state.Page + 1;
                var pageSize = state.PageSize;
                var response = await Api.GetPagedAsync(pageNo, pageSize, _searchQuery);

                _packagesList = response.Data;
                _currentPage = state.Page;
                _pageSize = state.PageSize;

                return new TableData<TravelPackageResponseModel>
                {
                    TotalItems = response.TotalCount,
                    Items = response.Data
                };
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading packages: {ex.Message}", Severity.Error);
            }

            return new TableData<TravelPackageResponseModel> { TotalItems = 0, Items = Array.Empty<TravelPackageResponseModel>() };
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

        private void OpenCreateDialog() => Nav.NavigateTo("/travel-package/create");

        private void OpenEditDialog(TravelPackageResponseModel package) => Nav.NavigateTo($"/travel-package/edit/{package.Id}");

        private async Task OpenDeleteDialog(long id)
        {
            var parameters = new DialogParameters { ["PackageId"] = id };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<TravelPackageDelete>("", parameters, options);
            var result = await dialog.Result;

            if (result is { Canceled: false })
            {
                _table?.ReloadServerData();
            }
        }
    }
}
