using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.BusTypes;

[ApiController]
[Route("api/[controller]")]
public class BusTypeController : ControllerBase
{
    private readonly IBusTypeService _service;

    public BusTypeController(IBusTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetBusTypes()
    {
        var types = await _service.GetBusTypesAsync();
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBusType(long id)
    {
        var type = await _service.GetBusTypeByIdAsync(id);
        if (type == null) return NotFound();
        return Ok(type);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBusType([FromBody] BusTypeRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.TypeName)) return BadRequest("TypeName is required.");
        
        var created = await _service.CreateBusTypeAsync(request);
        return CreatedAtAction(nameof(GetBusType), new { id = created.Id }, created);
    }

    [HttpPut("{id}/updates")]
    public async Task<IActionResult> UpdateBusType(long id, [FromBody] BusTypeRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.TypeName)) return BadRequest("TypeName is required.");

        var success = await _service.UpdateBusTypeAsync(id, request);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}/delete")]
    public async Task<IActionResult> DeleteBusType(long id)
    {
        var success = await _service.DeleteBusTypeAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}
