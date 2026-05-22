using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DotNet8.EasyTrip.App.Client.Pages.BusType
{
    public partial class BusTypeCreate
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;

        private BusTypeRequestModel _busTypeModel = new();
        private bool _success = false;
        private bool _isSaving = false;
        private MudForm? _form;

        private void Cancel() => Nav.NavigateTo("/bus-type");

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
                var response = await Http.PostAsJsonAsync("api/BusType/create", _busTypeModel);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Bus Type created successfully!", Severity.Success);
                    Nav.NavigateTo("/bus-type");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to create bus type: {error}", Severity.Error);
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
