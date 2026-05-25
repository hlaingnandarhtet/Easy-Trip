using System;

namespace DotNet8.EasyTripBackendApi.Models
{
    public class PaymentMethodResponseModel
    {
        public long Id { get; set; }
        public string PaymentType { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class PaymentMethodRequestModel
    {
        public string PaymentType { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
    }

    public class BookingPaymentResponseModel
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public string PaymentType { get; set; } = null!;
        public string TransactionNo { get; set; } = null!;
        public string ScreenshotImage { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
