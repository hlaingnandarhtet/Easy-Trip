using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTrip.App.Client.Services;

namespace DotNet8.EasyTrip.App.Client.Pages.TravelPackage
{
    public partial class TravelPackageDelete
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
        [Inject] private TravelPackageApiService Api { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        [Parameter] public long PackageId { get; set; }

        private string? _packageName;
        private bool _isDeleting;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var package = await Api.GetByIdAsync(PackageId);
                _packageName = package?.PackageName;
            }
            catch
            {
                _packageName = null;
            }
        }

        private void Cancel() => MudDialog.Cancel();

        private async Task Confirm()
        {
            _isDeleting = true;
            try
            {
                var (success, error) = await Api.DeleteAsync(PackageId);
                if (success)
                {
                    Snackbar.Add("Travel package deleted.", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Snackbar.Add($"Delete failed: {error}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isDeleting = false;
            }
        }
    }
}
