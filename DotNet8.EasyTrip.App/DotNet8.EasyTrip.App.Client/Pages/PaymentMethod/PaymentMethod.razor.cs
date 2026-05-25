using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.PaymentMethod
{
    public partial class PaymentMethod
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;

        private List<PaymentMethodResponseModel> _paymentMethodsList = new();
        private List<PaymentMethodResponseModel> _filteredList = new();
        private string _searchString = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<List<PaymentMethodResponseModel>>("api/Payment");
                _paymentMethodsList = response ?? new();
                PerformSearch();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading payment methods: {ex.Message}", Severity.Error);
            }
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(_searchString))
            {
                _filteredList = _paymentMethodsList;
            }
            else
            {
                _filteredList = _paymentMethodsList
                    .Where(m => (m.PaymentType != null && m.PaymentType.Contains(_searchString, StringComparison.OrdinalIgnoreCase)) ||
                                (m.AccountName != null && m.AccountName.Contains(_searchString, StringComparison.OrdinalIgnoreCase)) ||
                                (m.AccountNumber != null && m.AccountNumber.Contains(_searchString, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
        }

        private void ResetFilters()
        {
            _searchString = "";
            PerformSearch();
        }

        private int GetRowNumber(PaymentMethodResponseModel context)
        {
            return _filteredList.IndexOf(context) + 1;
        }

        private void OpenCreatePage()
        {
            Nav.NavigateTo("/payment-methods/create");
        }

        private void OpenEditPage(PaymentMethodResponseModel method)
        {
            Nav.NavigateTo($"/payment-methods/edit/{method.Id}");
        }

        private async Task OpenDeleteDialog(long id)
        {
            var parameters = new DialogParameters { ["PaymentMethodId"] = id };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };
            var dialog = await DialogService.ShowAsync<PaymentMethodDelete>("Confirm Delete", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                await LoadData();
            }
        }
    }
}
