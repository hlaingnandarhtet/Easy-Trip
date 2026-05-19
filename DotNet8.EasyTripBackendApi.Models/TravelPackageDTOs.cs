using System;

namespace DotNet8.EasyTripBackendApi.Models
{
    public class TravelPackageRequestModel
    {
        public string PackageName { get; set; } = null!;
        public long? BusId { get; set; }
        public long? HotelId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public bool? TransferService { get; set; }
        public decimal PackagePrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public PackageStatus PackageStatus { get; set; } = PackageStatus.Active;
    }

    public class TravelPackageResponseModel
    {
        public long Id { get; set; }
        public string PackageName { get; set; } = null!;
        public long? BusId { get; set; }
        public string? BusName { get; set; } // Auto-resolved Bus details from join
        public long? HotelId { get; set; }
        public string? HotelName { get; set; } // Auto-resolved Hotel details from join
        public decimal? DiscountPercentage { get; set; }
        public bool? TransferService { get; set; }
        public decimal PackagePrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public PackageStatus PackageStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
