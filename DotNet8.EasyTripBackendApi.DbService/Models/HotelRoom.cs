using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class HotelRoom
{
    public long Id { get; set; }

    public long? HotelId { get; set; }

    public long? RoomTypeId { get; set; }

    public decimal PricePerNight { get; set; }

    public string? Amenities { get; set; }

    public int AvailableRooms { get; set; }

    public int? HotelStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Hotel? Hotel { get; set; }

    public virtual RoomType? RoomType { get; set; }
}
