using System;

namespace DotNet8.EasyTripBackendApi.Models
{
    public class BookingRequestModel
    {
        public long? UserId { get; set; }
        public string BookingType { get; set; } = null!; // "Bus", "Hotel", or "Package"

        // Conditional detail fields for the booking_details table
        public long? BusId { get; set; }
        public long? HotelRoomId { get; set; }
        public long? PackageId { get; set; }
        public string? SelectedSeats { get; set; } // e.g. 'A1,A2'
        public int Quantity { get; set; } // Rooms count for Hotel, Headcount for Package
        public DateOnly TravelDate { get; set; }
        public DateOnly? EndDate { get; set; } // Check-out Date for Hotel

        public decimal TotalAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
    }

    public class PublicBookingRequestModel : BookingRequestModel
    {
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class BookingResponseModel
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string BookingType { get; set; } = null!;
        public long? ItemId { get; set; } // Legacy support (resolved as primary item id)
        public string? ItemName { get; set; } // Legacy support (resolved name description)
        public DateTime? BookingDate { get; set; }
        public DateOnly TravelDate { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public BookingStatus BookingStatus { get; set; }
        /// <summary>Raw DB status code (1=Pending, 2=Confirmed, 3=Rejected).</summary>
        public int BookingStatusCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Combined Detailed response representing the booking_details table records
        public BookingDetailResponseModel? Details { get; set; }
    }

    public class BookingDetailResponseModel
    {
        public long Id { get; set; }
        public long? BusId { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public long? HotelRoomId { get; set; }
        public string? HotelRoomTypeName { get; set; } // e.g., "Suite Room"
        public string? HotelName { get; set; } // e.g., "Grand Palace"
        public long? PackageId { get; set; }
        public string? PackageName { get; set; }
        public string? SelectedSeats { get; set; }
        public int Quantity { get; set; }
        public DateOnly TravelDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}

