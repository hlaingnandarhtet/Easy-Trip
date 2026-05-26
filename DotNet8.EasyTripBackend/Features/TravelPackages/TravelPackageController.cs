using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;
using DotNet8.EasyTripBackendApi.DbService.Models;

namespace DotNet8.EasyTripBackend.Features.TravelPackages
{
    [ApiController]
    [Route("api/travelpackage")]
    public class TravelPackageController : ControllerBase
    {
        private readonly ITravelPackageService _travelPackageService;
        private readonly AppDbContext _context;

        public TravelPackageController(ITravelPackageService travelPackageService, AppDbContext context)
        {
            _travelPackageService = travelPackageService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<TravelPackageResponseModel>>> GetTravelPackages([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageNo < 1) pageNo = 1;
            if (pageSize < 1) pageSize = 10;

            var result = await _travelPackageService.GetTravelPackagesAsync(pageNo, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TravelPackageResponseModel>> GetTravelPackage(long id)
        {
            var result = await _travelPackageService.GetTravelPackageAsync(id);
            if (result == null) return NotFound("Travel package not found");
            return Ok(result);
        }

        [HttpGet("bus")]
        public async Task<ActionResult<System.Collections.Generic.List<BusSelectDto>>> GetBuses()
        {
            var buses = await _context.Buses
                .Where(b => b.DeletedAt == null)
                .Select(b => new BusSelectDto
                {
                    Id = b.Id,
                    BusName = b.BusName,
                    Class = b.BusClass,
                    Price = b.Price,
                    Seats = b.TotalSeats
                })
                .ToListAsync();
            return Ok(buses);
        }

        [HttpGet("hotel")]
        public async Task<ActionResult<System.Collections.Generic.List<HotelSelectDto>>> GetHotels()
        {
            var hotels = await _context.Hotels
                .Where(h => h.DeletedAt == null)
                .Select(h => new HotelSelectDto
                {
                    Id = h.Id,
                    HotelName = h.HotelName,
                    Price = h.HotelRooms.Any() ? h.HotelRooms.Min(r => r.PricePerNight) : 0
                })
                .ToListAsync();
            return Ok(hotels);
        }

        [HttpPost]
        public async Task<ActionResult<TravelPackageResponseModel>> CreatePackage([FromBody] TravelPackageCreateDto dto)
        {
            try
            {
                var package = new TravelPackage
                {
                    PackageName = dto.PackageName,
                    PackagePrice = dto.PackagePrice,
                    DiscountPercentage = dto.Discount,
                    DurationDays = dto.DurationDays ?? 0,
                    StartDate = DateTime.SpecifyKind(dto.StartDate ?? DateTime.Today, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(dto.EndDate ?? DateTime.Today.AddDays(dto.DurationDays ?? 0), DateTimeKind.Utc),
                    BusId = dto.BusId > 0 ? dto.BusId : null,
                    HotelId = dto.HotelId > 0 ? dto.HotelId : null,
                    PackageStatus = Enum.TryParse<PackageStatus>(dto.Status, true, out var statusEnum) ? (int)statusEnum : 0,
                    TransferService = dto.IsTransferIncluded,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TravelPackages.Add(package);
                await _context.SaveChangesAsync();

                var response = new TravelPackageResponseModel
                {
                    Id = package.Id,
                    PackageName = package.PackageName,
                    BusId = package.BusId,
                    HotelId = package.HotelId,
                    DiscountPercentage = package.DiscountPercentage,
                    TransferService = package.TransferService,
                    PackagePrice = package.PackagePrice,
                    StartDate = package.StartDate,
                    EndDate = package.EndDate,
                    DurationDays = package.DurationDays,
                    PackageStatus = (PackageStatus)(package.PackageStatus ?? 0),
                    CreatedAt = package.CreatedAt
                };

                return CreatedAtAction(nameof(GetTravelPackage), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                var details = ex.InnerException != null 
                    ? $"{ex.Message} Inner: {ex.InnerException.Message}" 
                    : ex.Message;
                return BadRequest(details);
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<TravelPackageResponseModel>> CreateTravelPackage([FromBody] TravelPackageRequestModel request)
        {
            try
            {
                var result = await _travelPackageService.CreateTravelPackageAsync(request);
                return CreatedAtAction(nameof(GetTravelPackage), new { id = result.Id }, result);
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
