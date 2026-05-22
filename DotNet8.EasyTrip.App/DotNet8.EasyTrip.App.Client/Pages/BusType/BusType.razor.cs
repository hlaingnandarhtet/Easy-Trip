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

namespace DotNet8.EasyTrip.App.Client.Pages.BusType
{
    public partial class BusType
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;

        private List<BusTypeResponseModel> _busTypesList = new();
        private List<BusTypeResponseModel> _filteredList = new();
        private string _searchString = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<List<BusTypeResponseModel>>("api/BusType");
                _busTypesList = response ?? new();
                PerformSearch();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading bus types: {ex.Message}", Severity.Error);
            }
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(_searchString))
            {
                _filteredList = _busTypesList;
            }
            else
            {
                _filteredList = _busTypesList
                    .Where(bt => bt.TypeName != null && bt.TypeName.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        private void ResetFilters()
        {
            _searchString = "";
            PerformSearch();
        }

        private int GetRowNumber(BusTypeResponseModel context)
        {
            return _filteredList.IndexOf(context) + 1;
        }

        private void OpenCreatePage()
        {
            Nav.NavigateTo("/bus-type/create");
        }

        private void OpenEditPage(BusTypeResponseModel busType)
        {
            Nav.NavigateTo($"/bus-type/edit/{busType.Id}");
        }

        private async Task OpenDeleteDialog(long id)
        {
            var parameters = new DialogParameters { ["BusTypeId"] = id };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<BusTypeDelete>("Confirm Delete", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                await LoadData();
            }
        }
    }
}