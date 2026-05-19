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
    }
}
