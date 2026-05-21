using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.Hotel
{
    public partial class HotelCreate
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        private HotelRequestModel _hotelModel = new();
        private bool _success = false;
        private MudForm? _form;

        private void Cancel() => Nav.NavigateTo("/hotel");

        private async Task Submit()
        {
            if (_form != null)
            {
                await _form.ValidateAsync();
                if (!_form.IsValid) return;
            }

            try
            {
                var response = await Http.PostAsJsonAsync("api/Hotel/create", _hotelModel);

                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Hotel added successfully!", Severity.Success);
                    Nav.NavigateTo("/hotel");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to save hotel: {errorMsg}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
        }
    }
}
