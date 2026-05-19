using System;

namespace DotNet8.EasyTripBackend.Features.Bus
{
    public class BusRequestModel
    {
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public string? BusClass { get; set; }
        public int? TotalSeats { get; set; }
        public decimal? Price { get; set; }
        public string? StartPoint { get; set; }
        public string? EndPoint { get; set; }
        public string? Departure { get; set; }
        public string? Arrival { get; set; }
        public string? DriverName { get; set; }
        public string? TripType { get; set; }
        public string? TimeSlot { get; set; }
        public int? BusStatus { get; set; }
    }

    public class BusResponseModel
    {
        public long Id { get; set; }
        public string? BusName { get; set; }
        public string? BusNumber { get; set; }
        public string? BusClass { get; set; }
        public int? TotalSeats { get; set; }
        public decimal? Price { get; set; }
        public string? StartPoint { get; set; }
        public string? EndPoint { get; set; }
        public string? Departure { get; set; }
        public string? Arrival { get; set; }
        public string? DriverName { get; set; }
        public string? TripType { get; set; }
        public string? TimeSlot { get; set; }
        public int? BusStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
