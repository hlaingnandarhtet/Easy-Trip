using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;
using System.Globalization;
using System.Net.Http.Json;

namespace DotNet8.EasyTrip.App.Client.Pages.Public
{
    public partial class Home
    {
        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;

        // One-week date strip state
        private DateTime _weekStart = GetWeekStart(DateTime.Today);
        private DateTime _selectedDate = DateTime.Today;

        private static DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }

        private IEnumerable<DateTime> WeekDays =>
            Enumerable.Range(0, 7).Select(i => _weekStart.AddDays(i));

        private int GetBusCountForDay(DateTime day) =>
            GetFilteredBusesForDay(day).Count();

        private static readonly string[] DepartureDateFormats =
        {
            "dd MMM yyyy, hh:mm tt",
            "dd MMM yyyy, h:mm tt",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-dd"
        };

        private IEnumerable<BusResponseModel> GetFilteredBusesForDay(DateTime day) =>
            ApplyRouteFilters(_allBuses).Where(b => MatchesDepartureDay(b, day));

        private IEnumerable<BusResponseModel> ApplyRouteFilters(IEnumerable<BusResponseModel> source)
        {
            var searchFrom = _fromCity?.Trim();
            var searchTo = _toCity?.Trim();
            var searchClass = _busClass?.Trim();

            return source.Where(b =>
                (string.IsNullOrEmpty(searchFrom) || (b.StartPoint?.Trim().Equals(searchFrom, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                (string.IsNullOrEmpty(searchTo) || (b.EndPoint?.Trim().Equals(searchTo, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                (string.IsNullOrEmpty(searchClass) || (b.BusClass?.Trim().Equals(searchClass, StringComparison.OrdinalIgnoreCase) ?? false)));
        }

        private static DateTime? GetDepartureDate(BusResponseModel bus)
        {
            if (string.IsNullOrWhiteSpace(bus.Departure))
                return null;

            if (DateTime.TryParseExact(bus.Departure, DepartureDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
                return exact.Date;

            if (DateTime.TryParse(bus.Departure, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed.Date;

            if (DateTime.TryParse(bus.Departure, out var fallback))
                return fallback.Date;

            return null;
        }

        private static bool IsBusDeparted(BusResponseModel bus)
        {
            if (string.IsNullOrWhiteSpace(bus.Departure))
                return false;

            if (DateTime.TryParseExact(bus.Departure, DepartureDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
                return exact <= DateTime.Now;

            if (DateTime.TryParse(bus.Departure, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed <= DateTime.Now;

            if (DateTime.TryParse(bus.Departure, out var fallback))
                return fallback <= DateTime.Now;

            return false;
        }

        private static bool MatchesDepartureDay(BusResponseModel bus, DateTime day)
        {
            var departure = GetDepartureDate(bus);
            // No departure date set — still show the bus
            if (!departure.HasValue)
                return true;

            return departure.Value == day.Date;
        }

        private void SelectBestDayInWeek()
        {
            var today = DateTime.Today;
            var upcoming = WeekDays.FirstOrDefault(d => d.Date >= today && GetBusCountForDay(d) > 0);
            if (upcoming != default)
            {
                _selectedDate = upcoming;
                return;
            }

            var anyDay = WeekDays.FirstOrDefault(d => GetBusCountForDay(d) > 0);
            _selectedDate = anyDay != default ? anyDay : today;
        }

        private void SelectDay(DateTime day)
        {
            _selectedDate = day.Date;
            _departureDate = day.Date;
            SearchBuses();
        }

        private void PrevWeek()
        {
            _weekStart = _weekStart.AddDays(-7);
            if (_selectedDate < _weekStart || _selectedDate >= _weekStart.AddDays(7))
                SelectDay(_weekStart);
            else
                SearchBuses();
        }

        private void NextWeek()
        {
            _weekStart = _weekStart.AddDays(7);
            if (_selectedDate < _weekStart || _selectedDate >= _weekStart.AddDays(7))
                SelectDay(_weekStart);
            else
                SearchBuses();
        }

        private async Task ScrollBusCards(string scrollId, int direction)
        {
            try
            {
                await JS.InvokeVoidAsync("easyTripScroll.scrollById", scrollId, direction * 320);
            }
            catch (JSException)
            {
                // Script not loaded yet — ignore so UI does not break
            }
        }

        private bool isLoading = true;
        private string _activeTab = "Bus";
        private string _tripType = "One Way";
        private string _offerTab = "Bus";
        
        // Bus Search States
        private string _fromCity = "";
        private string _toCity = "";
        private DateTime? _departureDate = DateTime.Today;
        private DateTime? _returnDate = DateTime.Today.AddDays(2);
        private int _travellers = 1;
        private string _busClass = "";
        
        // Hotel Search States
        private string _hotelCity = "";
        private DateTime? _checkInDate = DateTime.Today;
        private DateTime? _checkOutDate = DateTime.Today.AddDays(1);
        private int _rooms = 1;
        private int _adults = 2;
        private int _children = 0;

        // Original Data
        private List<BusResponseModel> _allBuses = new();
        private List<HotelResponseModel> _allHotels = new();

        // Display Data
        private List<BusResponseModel> buses = new();
        private List<HotelResponseModel> hotels = new();
        private List<TravelPackageResponseModel> packages = new();

        // Dropdown Source Data
        private List<string> _busFromCities = new();
        private List<string> _busToCities = new();
        private List<string> _hotelCities = new();

        private class SpecialFare { public string Name { get; set; } = ""; public string Desc { get; set; } = ""; public bool Active { get; set; } }
        private List<SpecialFare> _specialFares = new()
        {
            new() { Name = "Regular",        Desc = "Standard rate"       },
            new() { Name = "Defence Forces", Desc = "Up to 500 mmk Off"     },
            new() { Name = "Students",       Desc = "Extra baggage, discount" },
            new() { Name = "Senior Citizens",Desc = "Up to 500 mmk Off"     },
            new() { Name = "Doctors/Nurses", Desc = "Up to 500 mmk Off"     },
        };

        private class Feature { public string Icon { get; set; } = ""; public string Title { get; set; } = ""; public string Desc { get; set; } = ""; public string BgColor { get; set; } = ""; }
        private List<Feature> _features = new()
        {
            new() { Icon="💳", Title="Lowest Prices",     Desc="We compare all operators to get you the best deal every time.",   BgColor="#dbeafe" },
            new() { Icon="🔒", Title="Secure Booking",    Desc="Your payment and personal data are always fully protected.",       BgColor="#d1fae5" },
            new() { Icon="🚌", Title="Wide Coverage",     Desc="Hundreds of routes across every major city in Myanmar.",           BgColor="#ffedd5" },
            new() { Icon="🎧", Title="24/7 Support",      Desc="Our dedicated team is available anytime to help you.",            BgColor="#fae8ff" },
        };

        private void ToggleFare(SpecialFare fare) => fare.Active = !fare.Active;
        
        private void SwapCities() 
        { 
            var tmp = _fromCity; 
            _fromCity = _toCity; 
            _toCity = tmp; 
        }

        private void SearchBuses() 
        {
            if (_departureDate.HasValue)
            {
                _selectedDate = _departureDate.Value.Date;
                _weekStart = GetWeekStart(_selectedDate);
            }

            // Filter by route/class, then by departure date for the selected day
            buses = ApplyRouteFilters(_allBuses)
                .Where(b => MatchesDepartureDay(b, _selectedDate))
                .ToList();
        }

        private void SearchHotels()
        {
            var searchCity = _hotelCity?.Trim();
            hotels = _allHotels.Where(h => 
                (string.IsNullOrEmpty(searchCity) || (h.Location?.Trim().Equals(searchCity, StringComparison.OrdinalIgnoreCase) ?? false))
            ).ToList();
        }

        private void BookNow(string type, long id) => Navigation.NavigateTo($"/booking/wizard?type={type}&id={id}");

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var busResponse = await Http.GetFromJsonAsync<PaginationResponse<BusResponseModel>>("api/bus?pageNo=1&pageSize=50");
                if (busResponse?.Data != null) 
                {
                    _allBuses = busResponse.Data.ToList();
                    buses = _allBuses.ToList();
                    
                    // Extract unique origins and destinations
                    _busFromCities = _allBuses.Select(b => b.StartPoint?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()!;
                    _busToCities = _allBuses.Select(b => b.EndPoint?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()!;
                    
                    // Keep from/to empty so all buses show until user searches
                    SelectBestDayInWeek();
                    SearchBuses();
                }

                var hotelResponse = await Http.GetFromJsonAsync<PaginationResponse<HotelResponseModel>>("api/hotel?pageNo=1&pageSize=50");
                if (hotelResponse?.Data != null) 
                {
                    _allHotels = hotelResponse.Data.ToList();
                    hotels = _allHotels.ToList();
                    
                    // Extract unique hotel cities/locations
                    _hotelCities = _allHotels.Select(h => h.Location?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()!;
                    if (_hotelCities.Any() && string.IsNullOrEmpty(_hotelCity)) _hotelCity = _hotelCities.First();
                }

                var pkgResponse = await Http.GetFromJsonAsync<PaginationResponse<TravelPackageResponseModel>>("api/travelpackage?pageNo=1&pageSize=6");
                if (pkgResponse?.Data != null) 
                {
                    packages = pkgResponse.Data.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
