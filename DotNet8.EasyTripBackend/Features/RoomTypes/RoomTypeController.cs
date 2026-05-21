using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.RoomTypes
{
    [ApiController]
    [Route("api/roomtypes")]
    public class RoomTypeController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;

        public RoomTypeController(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoomTypeResponseModel>>> GetRoomTypes()
        {
            var result = await _roomTypeService.GetRoomTypesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomTypeResponseModel>> GetRoomType(long id)
        {
            var result = await _roomTypeService.GetRoomTypeByIdAsync(id);
            if (result == null) return NotFound("Room type not found");
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<RoomTypeResponseModel>> CreateRoomType([FromBody] RoomTypeRequestModel request)
        {
            var result = await _roomTypeService.CreateRoomTypeAsync(request);
            return CreatedAtAction(nameof(GetRoomType), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoomTypeResponseModel>> UpdateRoomType(long id, [FromBody] RoomTypeRequestModel request)
        {
            var success = await _roomTypeService.UpdateRoomTypeAsync(id, request);
            if (!success) return NotFound("Room type not found");
            return Ok(await _roomTypeService.GetRoomTypeByIdAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomType(long id)
        {
            var success = await _roomTypeService.DeleteRoomTypeAsync(id);
            if (!success) return NotFound("Room type not found");
            return NoContent();
        }
    }
}
