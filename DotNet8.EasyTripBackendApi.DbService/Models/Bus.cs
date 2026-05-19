using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class Bus
{
    public long Id { get; set; }

    public string BusName { get; set; } = null!;

    public string BusNumber { get; set; } = null!;

    public string DriverName { get; set; } = null!;

    public string TripType { get; set; } = null!;

    public string BusClass { get; set; } = null!;

    public string Departure { get; set; } = null!;

    public string Arrival { get; set; } = null!;

    public string StartPoint { get; set; } = null!;

    public string EndPoint { get; set; } = null!;

    public string TimeSlot { get; set; } = null!;

    public decimal Price { get; set; }

    public int TotalSeats { get; set; }

    public int? BusStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<BusSeat> BusSeats { get; set; } = new List<BusSeat>();

    public virtual ICollection<TravelPackage> TravelPackages { get; set; } = new List<TravelPackage>();
}
