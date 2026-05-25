using System;

namespace DotNet8.EasyTripBackendApi.DbService.Models;

public partial class PaymentMethod
{
    public long Id { get; set; }

    public string PaymentType { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
