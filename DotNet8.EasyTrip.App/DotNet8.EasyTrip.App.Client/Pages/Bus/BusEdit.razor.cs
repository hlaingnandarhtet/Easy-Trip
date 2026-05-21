using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.Bus
{
    public partial class BusEdit
    {
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;

        [Parameter] public long Id { get; set; }

        private BusRequestModel _busModel = new();
        private bool _success = false;
        private MudForm? _form;
        private string? _departureString;
        private string? _arrivalString;
        private List<BusTypeResponseModel> _busTypes = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var types = await Http.GetFromJsonAsync<List<BusTypeResponseModel>>("api/BusType");
                _busTypes = types ?? new();

                var bus = await Http.GetFromJsonAsync<BusResponseModel>($"api/Bus/{Id}");
                if (bus != null)
                {
                    _busModel = new BusRequestModel
                    {
                        BusName = bus.BusName,
                        BusNumber = bus.BusNumber,
                        BusClass = bus.BusClass,
                        TotalSeats = bus.TotalSeats,
                        Price = bus.Price,
                        StartPoint = bus.StartPoint,
                        EndPoint = bus.EndPoint,
                        Departure = bus.Departure,
                        Arrival = bus.Arrival,
                        DriverName = bus.DriverName,
                        TripType = bus.TripType,
                        TimeSlot = bus.TimeSlot,
                        BusStatus = bus.BusStatus,
                        BusTypeId = bus.BusTypeId
                    };
                    _departureString = ParseToIsoString(bus.Departure);
                    _arrivalString = ParseToIsoString(bus.Arrival);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading bus details: {ex.Message}", Severity.Error);
                Nav.NavigateTo("/bus");
            }
        }

        private void OnBusTypeChanged(long? busTypeId)
        {
            _busModel.BusTypeId = busTypeId;
            if (busTypeId.HasValue)
            {
                _busModel.BusName = _busTypes.FirstOrDefault(t => t.Id == busTypeId.Value)?.TypeName;
            }
            else
            {
                _busModel.BusName = null;
            }
        }

        private void Cancel() => Nav.NavigateTo("/bus");

        private async Task Submit()
        {
            if (_form != null)
            {
                await _form.ValidateAsync();
                if (!_form.IsValid) return;
            }

            try
            {
                _busModel.Departure = FormatDateTimeString(_departureString);
                _busModel.Arrival = FormatDateTimeString(_arrivalString);

                var response = await Http.PutAsJsonAsync($"api/Bus/{Id}/update", _busModel);

                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Bus updated successfully!", Severity.Success);
                    Nav.NavigateTo("/bus");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to save bus: {errorMsg}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
        }

        private string? ParseToIsoString(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;
            if (DateTime.TryParse(dateStr, out var parsedDateTime))
            {
                return parsedDateTime.ToString("yyyy-MM-ddTHH:mm");
            }
            return null;
        }

        private string? FormatDateTimeString(string? dtString)
        {
            if (string.IsNullOrWhiteSpace(dtString)) return null;
            if (DateTime.TryParse(dtString, out var dt))
            {
                return dt.ToString("dd-MM-yyyy, hh:mm tt");
            }
            return dtString; // Fallback
        }
    }
}
