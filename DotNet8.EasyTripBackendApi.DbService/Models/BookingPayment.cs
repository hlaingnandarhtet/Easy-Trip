using System;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class BookingPayment
{
    public long Id { get; set; }

    public long BookingId { get; set; }

    public string PaymentType { get; set; } = null!;

    public string TransactionNo { get; set; } = null!;

    public string ScreenshotImage { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
