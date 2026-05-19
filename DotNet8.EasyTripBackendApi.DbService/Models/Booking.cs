using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class Booking
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string BookingType { get; set; } = null!;

    public long? ItemId { get; set; }

    public DateTime? BookingDate { get; set; }

    public DateOnly TravelDate { get; set; }

    public decimal TotalAmount { get; set; }

    public int? PaymentStatus { get; set; }

    public int? BookingStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
}

