using Microsoft.AspNetCore.Components;
using MudBlazor;
using DotNet8.EasyTrip.App.Client.Services;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTrip.App.Client.Pages.TravelPackage
{
    public partial class TravelPackageEdit
    {
        [Inject] private TravelPackageApiService Api { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        [Parameter] public long Id { get; set; }

        private TravelPackageRequestModel _packageModel = new();
        private bool _isSaving;
        private bool _isLoading = true;
        private MudForm? _form;

        private List<BusResponseModel> _buses = new();
        private List<HotelResponseModel> _hotels = new();

        private DateTime? _startDate;
        private DateTime? _endDate;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var busTask = Api.GetBusesForSelectAsync();
                var hotelTask = Api.GetHotelsForSelectAsync();
                var packageTask = Api.GetByIdAsync(Id);

                await Task.WhenAll(busTask, hotelTask, packageTask);

                _buses = busTask.Result;
                _hotels = hotelTask.Result;

                var package = packageTask.Result;
                if (package == null)
                {
                    Snackbar.Add("Travel package not found.", Severity.Error);
                    Nav.NavigateTo("/travel-package");
                    return;
                }

                _packageModel = new TravelPackageRequestModel
                {
                    PackageName = package.PackageName,
                    BusId = package.BusId,
                    HotelId = package.HotelId,
                    DiscountPercentage = package.DiscountPercentage,
                    TransferService = package.TransferService,
                    PackagePrice = package.PackagePrice,
                    StartDate = package.StartDate,
                    EndDate = package.EndDate,
                    DurationDays = package.DurationDays,
                    PackageStatus = package.PackageStatus
                };

                _startDate = package.StartDate;
                _endDate = package.EndDate;
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading package: {ex.Message}", Severity.Error);
                Nav.NavigateTo("/travel-package");
            }
            finally
            {
                _isLoading = false;
            }
        }

        // When Start Date changes → keep Duration, recalculate End Date
        private void OnStartDateChanged(DateTime? newStart)
        {
            _startDate = newStart;
            if (_startDate.HasValue)
            {
                _endDate = _startDate.Value.AddDays(_packageModel.DurationDays);
            }
            SyncModelDates();
        }

        // When End Date changes → recalculate Duration from dates
        private void OnEndDateChanged(DateTime? newEnd)
        {
            _endDate = newEnd;
            if (_startDate.HasValue && _endDate.HasValue && _endDate >= _startDate)
            {
                _packageModel.DurationDays = Math.Max(1, (_endDate.Value - _startDate.Value).Days);
            }
            SyncModelDates();
        }

        // When Duration field changes → keep Start Date, recalculate End Date
        private void OnDurationChanged(int newDuration)
        {
            _packageModel.DurationDays = Math.Max(1, newDuration);
            if (_startDate.HasValue)
            {
                _endDate = _startDate.Value.AddDays(_packageModel.DurationDays);
            }
            SyncModelDates();
        }

        private void SyncModelDates()
        {
            _packageModel.StartDate = _startDate ?? DateTime.Today;
            _packageModel.EndDate = _endDate ?? _packageModel.StartDate.AddDays(_packageModel.DurationDays);
        }

        private void Cancel() => Nav.NavigateTo("/travel-package");

        private async Task Submit()
        {
            if (_form != null)
            {
                await _form.ValidateAsync();
                if (!_form.IsValid) return;
            }

            if (_startDate == null || _endDate == null)
            {
                Snackbar.Add("Please select both Start Date and End Date.", Severity.Error);
                return;
            }

            if (_endDate < _startDate)
            {
                Snackbar.Add("End Date cannot be before Start Date.", Severity.Error);
                return;
            }

            SyncModelDates();

            _isSaving = true;
            try
            {
                var result = await Api.UpdateAsync(Id, _packageModel);
                if (result.Success)
                {
                    Snackbar.Add("Travel package updated successfully!", Severity.Success);
                    Nav.NavigateTo("/travel-package");
                }
                else
                {
                    Snackbar.Add($"Failed to update package: {result.Error}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isSaving = false;
            }
        }
    }
}
