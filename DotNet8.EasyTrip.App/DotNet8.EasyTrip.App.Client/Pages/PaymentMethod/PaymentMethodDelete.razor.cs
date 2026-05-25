using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DotNet8.EasyTrip.App.Client.Pages.PaymentMethod
{
    public partial class PaymentMethodDelete
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter] public long PaymentMethodId { get; set; }

        private bool _isDeleting = false;

        private void Cancel() => MudDialog.Cancel();

        private async Task Confirm()
        {
            _isDeleting = true;
            try
            {
                var response = await Http.DeleteAsync($"api/Payment/{PaymentMethodId}/delete");
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Payment Method deleted successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to delete payment method: {error}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred while deleting: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isDeleting = false;
            }
        }
    }
}
