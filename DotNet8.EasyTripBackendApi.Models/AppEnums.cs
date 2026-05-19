namespace DotNet8.EasyTripBackendApi.Models 
{
    public enum BusStatus { Available = 0, Full = 1, Cancelled = 2 }
    public enum HotelStatus { Available = 0, Full = 1 }
    public enum PaymentStatus { Unpaid = 0, UnderReview = 1, Paid = 2 }
    public enum BookingStatus { Pending = 0, Confirmed = 1, Cancelled = 2, Completed = 3 }
    public enum PackageStatus { Active = 0, Inactive = 1, Expired = 2 }
}