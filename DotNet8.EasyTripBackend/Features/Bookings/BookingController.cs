using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Bookings
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<BookingResponseModel>>> GetBookings([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNo < 1) pageNo = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _bookingService.GetBookingsAsync(pageNo, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseModel>> GetBooking(long id)
        {
            var result = await _bookingService.GetBookingByIdAsync(id);
            if (result == null) return NotFound("Booking not found");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<BookingResponseModel>> CreateBooking([FromBody] BookingRequestModel request)
        {
            try
            {
                var result = await _bookingService.CreateBookingAsync(request);
                return CreatedAtAction(nameof(GetBooking), new { id = result.Id }, result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/update")]
        public async Task<ActionResult<BookingResponseModel>> UpdateBooking(long id, [FromBody] BookingRequestModel request)
        {
            try
            {
                var result = await _bookingService.UpdateBookingAsync(id, request);
                if (result == null) return NotFound("Booking not found");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<BookingResponseModel>> UpdateStatus(long id, [FromBody] UpdateBookingStatusRequest request)
        {
            try
            {
                var result = await _bookingService.UpdatePaymentAndBookingStatusAsync(id, request.PaymentStatus, request.BookingStatus);
                if (result == null) return NotFound("Booking not found");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeleteBooking(long id)
        {
            var success = await _bookingService.DeleteBookingAsync(id);
            if (!success) return NotFound("Booking not found");
            return NoContent();
        }
    }

    public class UpdateBookingStatusRequest
    {
        public int PaymentStatus { get; set; }
        public int BookingStatus { get; set; }
    }
}
