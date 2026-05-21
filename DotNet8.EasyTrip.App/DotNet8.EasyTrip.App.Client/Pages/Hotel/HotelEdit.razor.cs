using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.Hotel
{
    public partial class HotelEdit
    {
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        [Parameter] public long Id { get; set; }

        private HotelRequestModel _hotelModel = new();
        private bool _success = false;
        private MudForm? _form;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var hotel = await Http.GetFromJsonAsync<HotelResponseModel>($"api/Hotel/{Id}");
                if (hotel != null)
                {
                    _hotelModel = new HotelRequestModel
                    {
                        HotelName = hotel.HotelName,
                        Location = hotel.Location
                    };
                }
                else
                {
                    Snackbar.Add("Hotel not found.", Severity.Error);
                    Nav.NavigateTo("/hotel");
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading hotel: {ex.Message}", Severity.Error);
                Nav.NavigateTo("/hotel");
            }
        }

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
                var response = await Http.PutAsJsonAsync($"api/Hotel/{Id}/update", _hotelModel);

                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Hotel updated successfully!", Severity.Success);
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
