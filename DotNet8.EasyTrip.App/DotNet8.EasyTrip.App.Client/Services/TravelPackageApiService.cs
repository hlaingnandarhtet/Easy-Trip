using System.Net.Http.Json;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTrip.App.Client.Services;

public class TravelPackageApiService
{
    private readonly HttpClient _http;

    public TravelPackageApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PaginationResponse<TravelPackageResponseModel>> GetPagedAsync(int pageNo, int pageSize, string? search = null)
    {
        var url = $"api/TravelPackage?pageNo={pageNo}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        var result = await _http.GetFromJsonAsync<PaginationResponse<TravelPackageResponseModel>>(url);
        return result ?? new PaginationResponse<TravelPackageResponseModel>();
    }

    public async Task<TravelPackageResponseModel?> GetByIdAsync(long id)
    {
        return await _http.GetFromJsonAsync<TravelPackageResponseModel>($"api/TravelPackage/{id}");
    }

    public async Task<List<BusResponseModel>> GetBusesForSelectAsync()
    {
        var result = await _http.GetFromJsonAsync<PaginationResponse<BusResponseModel>>("api/Bus?pageNo=1&pageSize=500");
        return result?.Data ?? new List<BusResponseModel>();
    }

    public async Task<List<HotelResponseModel>> GetHotelsForSelectAsync()
    {
        var result = await _http.GetFromJsonAsync<PaginationResponse<HotelResponseModel>>("api/Hotel?pageNo=1&pageSize=500");
        return result?.Data ?? new List<HotelResponseModel>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(TravelPackageRequestModel model)
    {
        var response = await _http.PostAsJsonAsync("api/TravelPackage/create", model);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(long id, TravelPackageRequestModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/TravelPackage/{id}/update", model);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await response.Content.ReadAsStringAsync());
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(long id)
    {
        var response = await _http.DeleteAsync($"api/TravelPackage/{id}/delete");
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await response.Content.ReadAsStringAsync());
    }
}
