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
    public partial class BusTypeDelete
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter] public long BusTypeId { get; set; }

        private bool _isDeleting = false;

        private void Cancel() => MudDialog.Cancel();

        private async Task Confirm()
        {
            _isDeleting = true;
            try
            {
                var response = await Http.DeleteAsync($"api/BusType/{BusTypeId}/delete");
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Bus Type deleted successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to delete bus type: {error}", Severity.Error);
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
