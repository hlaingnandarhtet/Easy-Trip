using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.PaymentMethod
{
    public partial class PaymentMethodEdit
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        [Parameter] public long Id { get; set; }

        private PaymentMethodRequestModel _paymentMethodModel = new();
        private bool _success = false;
        private bool _isLoading = true;
        private bool _isSaving = false;
        private MudForm? _form;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<PaymentMethodResponseModel>($"api/Payment/{Id}");
                if (response != null)
                {
                    _paymentMethodModel = new PaymentMethodRequestModel
                    {
                        PaymentType = response.PaymentType,
                        AccountName = response.AccountName,
                        AccountNumber = response.AccountNumber
                    };
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading payment method: {ex.Message}", Severity.Error);
                Nav.NavigateTo("/payment-methods");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void Cancel() => Nav.NavigateTo("/payment-methods");

        private async Task Submit()
        {
            if (_form != null)
            {
                await _form.ValidateAsync();
                if (!_form.IsValid) return;
            }

            _isSaving = true;
            try
            {
                var response = await Http.PutAsJsonAsync($"api/Payment/{Id}/updates", _paymentMethodModel);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Payment Method updated successfully!", Severity.Success);
                    Nav.NavigateTo("/payment-methods");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to update payment method: {error}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isSaving = false;
            }
        }
    }
}
