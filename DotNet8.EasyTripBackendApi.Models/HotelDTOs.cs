using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.Models
{
    public class HotelRequestModel
    {
        public string? HotelName { get; set; }
        public string? Location { get; set; }
        public List<HotelRoomRequestModel> HotelRooms { get; set; } = new List<HotelRoomRequestModel>();
    }

    public class HotelResponseModel
    {
        public long Id { get; set; }
        public string? HotelName { get; set; }
        public string? Location { get; set; }
        public List<HotelRoomResponseModel> HotelRooms { get; set; } = new List<HotelRoomResponseModel>();
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class RoomTypeResponseModel
    {
        public long Id { get; set; }
        public string? TypeName { get; set; }
    }

    public class RoomTypeRequestModel
    {
        public string? TypeName { get; set; }
    }

    public class HotelRoomRequestModel
    {
        public long? RoomTypeId { get; set; }
        public decimal PricePerNight { get; set; }
        public string? Amenities { get; set; } // Comma-separated list e.g. "WiFi, Breakfast"
        public int AvailableRooms { get; set; }
        public HotelStatus HotelStatus { get; set; } = HotelStatus.Available;
    }

    public class HotelRoomResponseModel
    {
        public long Id { get; set; }
        public long? HotelId { get; set; }
        public long? RoomTypeId { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal PricePerNight { get; set; }
        public string? Amenities { get; set; }
        public int AvailableRooms { get; set; }
        public HotelStatus HotelStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
