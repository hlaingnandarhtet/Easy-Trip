using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.TravelPackages
{
    [ApiController]
    [Route("api/[controller]")]
    public class TravelPackageController : ControllerBase
    {
        private readonly ITravelPackageService _travelPackageService;

        public TravelPackageController(ITravelPackageService travelPackageService)
        {
            _travelPackageService = travelPackageService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<TravelPackageResponseModel>>> GetTravelPackages([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNo < 1) pageNo = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _travelPackageService.GetTravelPackagesAsync(pageNo, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TravelPackageResponseModel>> GetTravelPackage(long id)
        {
            var result = await _travelPackageService.GetTravelPackageAsync(id);
            if (result == null) return NotFound("Travel package not found");
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<TravelPackageResponseModel>> CreateTravelPackage([FromBody] TravelPackageRequestModel request)
        {
            try
            {
                var result = await _travelPackageService.CreateTravelPackageAsync(request);
                return CreatedAtAction(nameof(GetTravelPackage), new { id = result.Id }, result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/update")]
        public async Task<ActionResult<TravelPackageResponseModel>> UpdateTravelPackage(long id, [FromBody] TravelPackageRequestModel request)
        {
            try
            {
                var result = await _travelPackageService.UpdateTravelPackageAsync(id, request);
                if (result == null) return NotFound("Travel package not found");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeleteTravelPackage(long id)
        {
            var success = await _travelPackageService.DeleteTravelPackageAsync(id);
            if (!success) return NotFound("Travel package not found");
            return NoContent();
        }
    }
}
