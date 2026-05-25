using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.PaymentMethod
{
    public partial class PaymentMethodCreate
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        private PaymentMethodRequestModel _paymentMethodModel = new();
        private bool _success = false;
        private bool _isSaving = false;
        private MudForm? _form;

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
                var response = await Http.PostAsJsonAsync("api/Payment/create", _paymentMethodModel);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Payment Method created successfully!", Severity.Success);
                    Nav.NavigateTo("/payment-methods");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to create payment method: {error}", Severity.Error);
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
