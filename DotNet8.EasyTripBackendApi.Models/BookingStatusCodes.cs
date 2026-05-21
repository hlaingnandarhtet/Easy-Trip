namespace DotNet8.EasyTripBackendApi.Models
{
    /// <summary>
    /// Booking status values stored in the database (differs from AppEnums.BookingStatus indices).
    /// </summary>
    public static class BookingStatusCodes
    {
        public const int Pending = 1;
        public const int Confirmed = 2;
        public const int Rejected = 3;

        public static readonly int[] SeatBlockingStatuses = { Pending, Confirmed };
    }
}
