using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DotNet8.EasyTrip.App.Client.Pages.Hotel
{
    public partial class HotelDelete
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        [Parameter] public long HotelId { get; set; }

        private void Cancel() => MudDialog.Cancel();

        private async Task Confirm()
        {
            try
            {
                var response = await Http.DeleteAsync($"api/Hotel/{HotelId}/delete");
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Hotel deleted successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Snackbar.Add("Failed to delete hotel.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred while deleting: {ex.Message}", Severity.Error);
            }
        }
    }
}
