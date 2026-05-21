using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.Hotels
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<HotelResponseModel>>> GetHotels([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageNo < 1) pageNo = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _hotelService.GetHotelsAsync(pageNo, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HotelResponseModel>> GetHotel(long id)
        {
            var result = await _hotelService.GetHotelAsync(id);
            if (result == null) return NotFound("Hotel not found");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<HotelResponseModel>> CreateHotel([FromBody] HotelRequestModel request)
        {
            var result = await _hotelService.CreateHotelAsync(request);
            return CreatedAtAction(nameof(GetHotel), new { id = result.Id }, result);
        }

        [HttpPut("{id}/update")]
        public async Task<ActionResult<HotelResponseModel>> UpdateHotel(long id, [FromBody] HotelRequestModel request)
        {
            var result = await _hotelService.UpdateHotelAsync(id, request);
            if (result == null) return NotFound("Hotel not found");
            return Ok(result);
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeleteHotel(long id)
        {
            var success = await _hotelService.DeleteHotelAsync(id);
            if (!success) return NotFound("Hotel not found");
            return NoContent();
        }
    }
}
