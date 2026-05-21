using System;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;
using Microsoft.JSInterop;

namespace DotNet8.EasyTrip.App.Client.Pages.Public
{
    public partial class UserDashboard
    {
        private async Task DownloadInvoice(BookingResponseModel booking)
        {
            var itemDesc = GetItemDescription(booking);
            var statusLabel = GetStatusLabel(booking.BookingStatusCode);
            var paymentLabel = booking.PaymentStatus == PaymentStatus.Paid ? "Paid" : "Unpaid";
            var travelDate = booking.TravelDate.ToString("dd-MM-yyyy");
            var bookingDate = booking.BookingDate?.ToString("dd-MM-yyyy HH:mm") ?? "—";

            var endDateHtml = booking.Details?.EndDate != null 
                ? $"<div class='row'><span class='label'>Check-out Date</span><span class='value'>{booking.Details.EndDate?.ToString("dd-MM-yyyy")}</span></div>"
                : "";
                
            var seatsHtml = booking.Details?.SelectedSeats != null
                ? $"<div class='row'><span class='label'>Seat(s)</span><span class='value'>{booking.Details.SelectedSeats}</span></div>"
                : "";

            var htmlTemplate = @"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<title>Invoice INV-{bookingId}</title>
<style>
  body { font-family: 'Segoe UI', sans-serif; background:#f8fafc; margin:0; padding:0; }
  .page { max-width:680px; margin:40px auto; background:#fff; border-radius:16px; box-shadow:0 4px 32px rgba(0,0,0,0.10); overflow:hidden; }
  .header { background:linear-gradient(135deg,#0f172a,#10b981); padding:36px 40px; }
  .header h1 { color:#fff; margin:0; font-size:1.6rem; font-weight:800; }
  .header p { color:rgba(255,255,255,0.75); margin:4px 0 0; font-size:0.95rem; }
  .badge { display:inline-block; background:rgba(255,255,255,0.18); color:#fff; border-radius:99px; padding:4px 14px; font-size:0.8rem; font-weight:700; margin-top:10px; }
  .body { padding:36px 40px; }
  .section-title { font-size:0.75rem; font-weight:700; color:#64748b; letter-spacing:0.08em; text-transform:uppercase; margin-bottom:14px; }
  .row { display:flex; justify-content:space-between; padding:10px 0; border-bottom:1px solid #f1f5f9; }
  .row:last-child { border-bottom:none; }
  .label { color:#64748b; font-size:0.9rem; }
  .value { color:#0f172a; font-weight:600; font-size:0.9rem; text-align:right; }
  .amount-row { background:#f0fdf4; border-radius:10px; padding:14px 18px; margin-top:20px; display:flex; justify-content:space-between; align-items:center; }
  .amount-label { color:#059669; font-weight:700; font-size:1rem; }
  .amount-value { color:#059669; font-weight:800; font-size:1.4rem; }
  .status-confirmed { color:#059669; font-weight:700; }
  .footer { background:#f8fafc; padding:20px 40px; text-align:center; color:#94a3b8; font-size:0.8rem; border-top:1px solid #e2e8f0; }
  _media_print_ { body{background:#fff;} .page{box-shadow:none; margin:0;} }
</style>
</head>
<body>
<div class='page'>
  <div class='header'>
    <h1>EasyTrip Invoice</h1>
    <p>Booking Confirmation &amp; Invoice Slip</p>
    <span class='badge'>✓ CONFIRMED</span>
  </div>
  <div class='body'>
    <div class='section-title'>Invoice Details</div>
    <div class='row'><span class='label'>Invoice No.</span><span class='value'>#INV-{bookingId}</span></div>
    <div class='row'><span class='label'>Booking Date</span><span class='value'>{bookingDate}</span></div>
    <div class='row'><span class='label'>Status</span><span class='value status-confirmed'>✓ {statusLabel}</span></div>
    <div class='row'><span class='label'>Payment</span><span class='value'>{paymentLabel}</span></div>

    <div class='section-title' style='margin-top:28px;'>Booking Details</div>
    <div class='row'><span class='label'>Booking Type</span><span class='value'>{bookingType}</span></div>
    <div class='row'><span class='label'>Item</span><span class='value'>{itemDesc}</span></div>
    <div class='row'><span class='label'>Travel Date</span><span class='value'>{travelDate}</span></div>
    {endDateHtml}
    {seatsHtml}

    <div class='section-title' style='margin-top:28px;'>Passenger / Guest</div>
    <div class='row'><span class='label'>Name</span><span class='value'>{userName}</span></div>
    <div class='row'><span class='label'>Phone</span><span class='value'>{userPhone}</span></div>
    <div class='row'><span class='label'>Email</span><span class='value'>{userEmail}</span></div>

    <div class='amount-row'>
      <span class='amount-label'>Total Amount</span>
      <span class='amount-value'>Ks {totalAmount}</span>
    </div>
  </div>
  <div class='footer'>
    Thank you for choosing EasyTrip! Present this invoice at your point of departure. · easytrip.com
  </div>
</div>
<script>window.onload=function(){window.print();}</script>
</body>
</html>";

            var html = htmlTemplate
                .Replace("{bookingId}", booking.Id.ToString("D5"))
                .Replace("{bookingDate}", bookingDate)
                .Replace("{statusLabel}", statusLabel)
                .Replace("{paymentLabel}", paymentLabel)
                .Replace("{bookingType}", booking.BookingType)
                .Replace("{itemDesc}", itemDesc)
                .Replace("{travelDate}", travelDate)
                .Replace("{endDateHtml}", endDateHtml)
                .Replace("{seatsHtml}", seatsHtml)
                .Replace("{userName}", booking.UserName ?? "—")
                .Replace("{userPhone}", _phoneInput)
                .Replace("{userEmail}", booking.UserEmail ?? "—")
                .Replace("{totalAmount}", booking.TotalAmount.ToString("N0"))
                .Replace("_media_print_", "@media print");

            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            var base64 = Convert.ToBase64String(bytes);
            await JS.InvokeVoidAsync("easyTripInvoice.open", base64, $"INV-{booking.Id:D5}");
        }
    }
}
