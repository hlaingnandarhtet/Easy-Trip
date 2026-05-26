using System.Text.Json;
using DotNet8.EasyTripBackendApi.Models;
using Microsoft.JSInterop;

namespace DotNet8.EasyTrip.App.Client.Services;

public static class BookingTicketService
{
    public static string GetTicketReference(long bookingId) => $"ET-{bookingId:D8}";

    public static string BuildQrPayload(BookingResponseModel booking, string? phone = null)
    {
        var payload = new
        {
            app = "EasyTrip",
            v = 1,
            ticket = GetTicketReference(booking.Id),
            id = booking.Id,
            type = booking.BookingType,
            travel = booking.TravelDate.ToString("yyyy-MM-dd"),
            guest = booking.UserName,
            phone,
            amount = booking.TotalAmount,
            status = booking.BookingStatusCode
        };

        return JsonSerializer.Serialize(payload);
    }

    public static async Task<string> GenerateQrDataUrlAsync(
        IJSRuntime js, BookingResponseModel booking, string? phone = null, int size = 220)
    {
        var payload = BuildQrPayload(booking, phone);
        return await js.InvokeAsync<string>("easyTripQr.toDataUrl", payload, size);
    }
}
