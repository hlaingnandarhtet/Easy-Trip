using System;
using System.Collections.Generic;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class BusType
{
    public long Id { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();
}
