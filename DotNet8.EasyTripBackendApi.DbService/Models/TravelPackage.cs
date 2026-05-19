using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class TravelPackage
{
    public long Id { get; set; }

    public string PackageName { get; set; } = null!;

    public long? BusId { get; set; }

    public long? HotelId { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public bool? TransferService { get; set; }

    public decimal PackagePrice { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int DurationDays { get; set; }

    public int? PackageStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Bus? Bus { get; set; }

    public virtual Hotel? Hotel { get; set; }
}
