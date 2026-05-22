using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.Models
{
    // ─── Pending Dashboard ───────────────────────────────────────────────────────
    public class PendingDashboardModel
    {
        public int TotalPending { get; set; }
        public int BusPending { get; set; }
        public int HotelPending { get; set; }
        public int PackagePending { get; set; }
        public decimal TotalPendingRevenue { get; set; }
        public List<PendingBookingItem> PendingBookings { get; set; } = new();
    }

    public class PendingBookingItem
    {
        public long BookingId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string BookingType { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateOnly TravelDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatusText { get; set; } = string.Empty;
        public int PaymentStatus { get; set; }

        // Bus details (filled when BookingType == "Bus")
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public string? BusClass { get; set; }
        public string? Route { get; set; }
        public string? TimeSlot { get; set; }
        public string? SelectedSeats { get; set; }
        public int? SeatCount { get; set; }

        // Hotel details (filled when BookingType == "Hotel")
        public string? HotelName { get; set; }
        public string? HotelLocation { get; set; }
        public string? RoomTypeName { get; set; }
        public int? RoomQuantity { get; set; }
        public DateOnly? CheckIn { get; set; }
        public DateOnly? CheckOut { get; set; }
        public decimal? PricePerNight { get; set; }

        // Package details (filled when BookingType == "Package" / "TravelPackage")
        public string? PackageName { get; set; }
        public int? DurationDays { get; set; }
        public decimal? PackagePrice { get; set; }
        public string? PackageBusInfo { get; set; }
        public string? PackageHotelInfo { get; set; }
    }


    public class DashboardSummaryModel
    {
        public decimal TotalRevenue { get; set; }
        public string TotalRevenueText { get; set; } = "0 mmk";
        public double RevenueGrowthPercentage { get; set; } = 0.0;
        
        public int ActiveBookingsCount { get; set; }
        public int PendingBookingsCount { get; set; }
        
        public int BusTicketsCount { get; set; }
        public int HotelBookingsCount { get; set; }
        public int PackageBookingsCount { get; set; }
        
        public int BusesInTransit { get; set; }
        public int BusesInMaintenance { get; set; }
        public double BusEfficiency { get; set; }
        
        public double HotelOccupancy { get; set; }
        public int AvailableRoomsCount { get; set; }
        
        // Distribution ratios for Profit Margin Breakdown
        public double BusMarginRatio { get; set; } = 45.0;
        public double HotelMarginRatio { get; set; } = 30.0;
        public double PackageMarginRatio { get; set; } = 25.0;
        
        // Weekly analytical charts data
        public List<string> ChartLabels { get; set; } = new();
        public List<double> BusSalesSeries { get; set; } = new();
        public List<double> HotelSalesSeries { get; set; } = new();
        public List<double> PackageSalesSeries { get; set; } = new();
        
        // Recent live transactions activity
        public List<DashboardTransactionItem> RecentTransactions { get; set; } = new();
    }

    public class DashboardTransactionItem
    {
        public string TransactionId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Segment { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
