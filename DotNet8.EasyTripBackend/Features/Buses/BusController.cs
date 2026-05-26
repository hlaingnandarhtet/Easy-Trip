using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackend.Features.Bookings;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Bus
{
    [ApiController]
    [Route("api/bus")]
    public class BusController : ControllerBase
    {
        private readonly IBusService _busService;
        private readonly IBookingService _bookingService;

        public BusController(IBusService busService, IBookingService bookingService)
        {
            _busService = busService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<BusResponseModel>>> GetBuses([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageNo < 1) pageNo = 1;
            if (pageSize < 1) pageSize = 10;
            
            var result = await _busService.GetBusesAsync(pageNo, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BusResponseModel>> GetByIdBus(long id)
        {
            var result = await _busService.GetBusAsync(id);
            if (result == null) return NotFound("Bus not found");
            return Ok(result);
        }

        [HttpGet("{id}/reserved-seat")]
        public async Task<ActionResult<List<string>>> GetReservedSeats(long id, [FromQuery] DateOnly travelDate)
        {
            var bus = await _busService.GetBusAsync(id);
            if (bus == null) return NotFound("Bus not found");

            var seats = await _bookingService.GetReservedSeatsForBusAsync(id, travelDate);
            return Ok(seats);
        }

        [HttpPost("create")]
        public async Task<ActionResult<BusResponseModel>> CreateBus([FromBody] BusRequestModel request)
        {
            var result = await _busService.CreateBusAsync(request);
            return CreatedAtAction(nameof(GetByIdBus), new { id = result.Id }, result);
        }

        [HttpPut("{id}/update")]
        public async Task<ActionResult<BusResponseModel>> UpdateBus(long id, [FromBody] BusRequestModel request)
        {
            var result = await _busService.UpdateBusAsync(id, request);
            if (result == null) return NotFound("Bus not found");
            return Ok(result);
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeleteBus(long id)
        {
            var success = await _busService.DeleteBusAsync(id);
            if (!success) return NotFound("Bus not found");
            return NoContent();
        }
    }
}
