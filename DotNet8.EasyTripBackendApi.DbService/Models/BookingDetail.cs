using System;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class BookingDetail
{
    public long Id { get; set; }

    public long BookingId { get; set; }

    public long? BusId { get; set; }

    public long? HotelRoomId { get; set; }

    public long? PackageId { get; set; }

    public string? SelectedSeats { get; set; }

    public int Quantity { get; set; }

    public DateOnly TravelDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Bus? Bus { get; set; }

    public virtual HotelRoom? HotelRoom { get; set; }

    public virtual TravelPackage? Package { get; set; }
}
