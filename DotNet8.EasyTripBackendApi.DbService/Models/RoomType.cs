using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class RoomType
{
    public long Id { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<HotelRoom> HotelRooms { get; set; } = new List<HotelRoom>();
}
