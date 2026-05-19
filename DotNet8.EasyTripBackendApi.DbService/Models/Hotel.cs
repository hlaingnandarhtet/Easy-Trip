using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class Hotel
{
    public long Id { get; set; }

    public string HotelName { get; set; } = null!;

    public string Location { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<HotelRoom> HotelRooms { get; set; } = new List<HotelRoom>();

    public virtual ICollection<TravelPackage> TravelPackages { get; set; } = new List<TravelPackage>();
}
