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
    public partial class BusTypeEdit
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private IDialogService DialogService { get; set; } = null!;

        [Parameter] public long Id { get; set; }

        private BusTypeRequestModel _busTypeModel = new();
        private bool _success = false;
        private bool _isLoading = true;
        private bool _isSaving = false;
        private MudForm? _form;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<BusTypeResponseModel>($"api/BusType/{Id}");
                if (response != null)
                {
                    _busTypeModel = new BusTypeRequestModel
                    {
                        TypeName = response.TypeName
                    };
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading bus type: {ex.Message}", Severity.Error);
                Nav.NavigateTo("/bus-type");
            }
            finally
            {
                _isLoading = false;
            }
        }

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
                var response = await Http.PutAsJsonAsync($"api/BusType/{Id}/updates", _busTypeModel);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Bus Type updated successfully!", Severity.Success);
                    Nav.NavigateTo("/bus-type");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to update bus type: {error}", Severity.Error);
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
