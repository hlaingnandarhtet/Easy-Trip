using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class BusSeat
{
    public long Id { get; set; }

    public long? BusId { get; set; }

    public string SeatNo { get; set; } = null!;

    public bool? IsBooked { get; set; }

    public long? BookingId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Bus? Bus { get; set; }
}
