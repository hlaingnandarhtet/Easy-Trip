using System;

namespace DotNet8.EasyTripBackendApi.Models;

public class BusTypeRequestModel
{
    public string TypeName { get; set; } = null!;
}

public class BusTypeResponseModel
{
    public long Id { get; set; }
    public string TypeName { get; set; } = null!;
}
