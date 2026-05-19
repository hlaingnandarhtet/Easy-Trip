using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;

namespace DotNet8.EasyTripBackend.Features.Bus
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusController : ControllerBase
    {
        private readonly IBusService _busService;

        public BusController(IBusService busService)
        {
            _busService = busService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<BusResponseModel>>> GetBuses([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNo < 1) pageNo = 1;
            if (pageSize < 1) pageSize = 10;
            
            var result = await _busService.GetBusesAsync(pageNo, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BusResponseModel>> GetByIdBus(long id)
        {
            var result = await _busService.GetBusAsync(id);
            if (result == null) return NotFound("Bus not found");
            return Ok(result);
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
