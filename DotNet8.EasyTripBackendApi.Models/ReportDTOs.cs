using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.Models
{
    public class SalesRevenueReportModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PreviousPeriodRevenue { get; set; }
        public double RevenueGrowthPercentage { get; set; }
        public int PaidTransactionCount { get; set; }
        public List<RevenueByTypeItem> RevenueByType { get; set; } = new();
        public List<string> ChartLabels { get; set; } = new();
        public List<double> BusSalesSeries { get; set; } = new();
        public List<double> HotelSalesSeries { get; set; } = new();
        public List<double> PackageSalesSeries { get; set; } = new();
        public List<double> TotalSalesSeries { get; set; } = new();
    }

    public class RevenueByTypeItem
    {
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public int TransactionCount { get; set; }
    }

    public class BookingAnalyticsReportModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalBookings { get; set; }
        public int ConfirmedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int BusCount { get; set; }
        public int HotelCount { get; set; }
        public int PackageCount { get; set; }
        public decimal TotalBookingValue { get; set; }
        public decimal AverageBookingAmount { get; set; }
        public double ConversionRate { get; set; }
        public List<string> ChartLabels { get; set; } = new();
        public List<double> BookingVolumeSeries { get; set; } = new();
        public List<double> ConfirmedVolumeSeries { get; set; } = new();
        public List<double> PendingVolumeSeries { get; set; } = new();
    }

    public class TopServicesReportModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ServiceTypeFilter { get; set; } = "All";
        public string Metric { get; set; } = "revenue";
        public List<TopServiceItem> Services { get; set; } = new();
    }

    public class TopServiceItem
    {
        public int Rank { get; set; }
        public long ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
