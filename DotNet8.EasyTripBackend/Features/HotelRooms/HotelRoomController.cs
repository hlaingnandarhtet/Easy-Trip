using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.HotelRooms
{
    [ApiController]
    public class HotelRoomController : ControllerBase
    {
        private readonly IHotelRoomService _hotelRoomService;

        public HotelRoomController(IHotelRoomService hotelRoomService)
        {
            _hotelRoomService = hotelRoomService;
        }

        [HttpGet("api/hotel/{hotelId}/room")]
        public async Task<ActionResult<List<HotelRoomResponseModel>>> GetHotelRoomsByHotel(long hotelId)
        {
            var result = await _hotelRoomService.GetHotelRoomsByHotelAsync(hotelId);
            return Ok(result);
        }

        [HttpGet("api/room/{id}")]
        public async Task<ActionResult<HotelRoomResponseModel>> GetHotelRoom(long id)
        {
            var result = await _hotelRoomService.GetHotelRoomAsync(id);
            if (result == null) return NotFound("Hotel room not found");
            return Ok(result);
        }

        [HttpPost("api/hotel/{hotelId}/room/create")]
        public async Task<ActionResult<HotelRoomResponseModel>> CreateHotelRoom(long hotelId, [FromBody] HotelRoomRequestModel request)
        {
            try
            {
                var created = await _hotelRoomService.CreateHotelRoomAsync(hotelId, request);
                return CreatedAtAction(nameof(GetHotelRoom), new { id = created.Id }, created);
            }
            catch (System.Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("api/room/{id}/update")]
        public async Task<ActionResult<HotelRoomResponseModel>> UpdateHotelRoom(long id, [FromBody] HotelRoomRequestModel request)
        {
            try
            {
                var result = await _hotelRoomService.UpdateHotelRoomAsync(id, request);
                if (result == null) return NotFound("Hotel room not found");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("api/room/{id}/delete")]
        public async Task<IActionResult> DeleteHotelRoom(long id)
        {
            var success = await _hotelRoomService.DeleteHotelRoomAsync(id);
            if (!success) return NotFound("Hotel room not found");
            return NoContent();
        }
    }
}
