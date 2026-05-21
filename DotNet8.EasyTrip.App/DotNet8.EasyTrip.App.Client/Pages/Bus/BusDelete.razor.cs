using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DotNet8.EasyTrip.App.Client.Pages.Bus
{
    public partial class BusDelete
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        [Parameter] public long BusId { get; set; }

        private void Cancel() => MudDialog.Cancel();

        private async Task Confirm()
        {
            try
            {
                var response = await Http.DeleteAsync($"api/Bus/{BusId}/delete");
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Bus deleted successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Snackbar.Add("Failed to delete bus.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred while deleting: {ex.Message}", Severity.Error);
            }
        }
    }
}
