using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Shared;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.TravelPackages
{
    public class TravelPackageService : ITravelPackageService
    {
        private readonly AppDbContext _context;

        public TravelPackageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResponse<TravelPackageResponseModel>> GetTravelPackagesAsync(int pageNo, int pageSize)
        {
            var query = from tp in _context.TravelPackages
                        where tp.DeletedAt == null
                        join b in _context.Buses on tp.BusId equals b.Id into busJoin
                        from b in busJoin.DefaultIfEmpty()
                        join h in _context.Hotels on tp.HotelId equals h.Id into hotelJoin
                        from h in hotelJoin.DefaultIfEmpty()
                        orderby tp.Id descending
                        select new TravelPackageResponseModel
                        {
                            Id = tp.Id,
                            PackageName = tp.PackageName,
                            BusId = tp.BusId,
                            BusName = b != null ? b.BusName : null,
                            HotelId = tp.HotelId,
                            HotelName = h != null ? h.HotelName : null,
                            DiscountPercentage = tp.DiscountPercentage,
                            TransferService = tp.TransferService,
                            PackagePrice = tp.PackagePrice,
                            StartDate = tp.StartDate,
                            EndDate = tp.EndDate,
                            DurationDays = tp.DurationDays,
                            PackageStatus = (PackageStatus)(tp.PackageStatus ?? 0),
                            CreatedAt = tp.CreatedAt,
                            DeletedAt = tp.DeletedAt
                        };

            return await query.ToPagedListAsync(pageNo, pageSize);
        }

        public async Task<TravelPackageResponseModel?> GetTravelPackageAsync(long id)
        {
            var query = from tp in _context.TravelPackages
                        where tp.Id == id && tp.DeletedAt == null
                        join b in _context.Buses on tp.BusId equals b.Id into busJoin
                        from b in busJoin.DefaultIfEmpty()
                        join h in _context.Hotels on tp.HotelId equals h.Id into hotelJoin
                        from h in hotelJoin.DefaultIfEmpty()
                        select new TravelPackageResponseModel
                        {
                            Id = tp.Id,
                            PackageName = tp.PackageName,
                            BusId = tp.BusId,
                            BusName = b != null ? b.BusName : null,
                            HotelId = tp.HotelId,
                            HotelName = h != null ? h.HotelName : null,
                            DiscountPercentage = tp.DiscountPercentage,
                            TransferService = tp.TransferService,
                            PackagePrice = tp.PackagePrice,
                            StartDate = tp.StartDate,
                            EndDate = tp.EndDate,
                            DurationDays = tp.DurationDays,
                            PackageStatus = (PackageStatus)(tp.PackageStatus ?? 0),
                            CreatedAt = tp.CreatedAt,
                            DeletedAt = tp.DeletedAt
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<TravelPackageResponseModel> CreateTravelPackageAsync(TravelPackageRequestModel request)
        {
            // Validate relations
            if (request.BusId.HasValue)
            {
                var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId.Value && b.DeletedAt == null);
                if (!busExists) throw new Exception("Bus not found");
            }

            if (request.HotelId.HasValue)
            {
                var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == request.HotelId.Value && h.DeletedAt == null);
                if (!hotelExists) throw new Exception("Hotel not found");
            }

            var newPackage = new TravelPackage
            {
                PackageName = request.PackageName,
                BusId = request.BusId,
                HotelId = request.HotelId,
                DiscountPercentage = request.DiscountPercentage,
                TransferService = request.TransferService,
                PackagePrice = request.PackagePrice,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DurationDays = request.DurationDays,
                PackageStatus = (int)request.PackageStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.TravelPackages.Add(newPackage);
            await _context.SaveChangesAsync();

            return await GetTravelPackageAsync(newPackage.Id) ?? throw new Exception("Failed to retrieve created travel package");
        }

        public async Task<TravelPackageResponseModel?> UpdateTravelPackageAsync(long id, TravelPackageRequestModel request)
        {
            var existingPackage = await _context.TravelPackages.FirstOrDefaultAsync(tp => tp.Id == id && tp.DeletedAt == null);
            if (existingPackage == null) return null;

            // Validate relations
            if (request.BusId.HasValue)
            {
                var busExists = await _context.Buses.AnyAsync(b => b.Id == request.BusId.Value && b.DeletedAt == null);
                if (!busExists) throw new Exception("Bus not found");
            }

            if (request.HotelId.HasValue)
            {
                var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == request.HotelId.Value && h.DeletedAt == null);
                if (!hotelExists) throw new Exception("Hotel not found");
            }

            existingPackage.PackageName = request.PackageName;
            existingPackage.BusId = request.BusId;
            existingPackage.HotelId = request.HotelId;
            existingPackage.DiscountPercentage = request.DiscountPercentage;
            existingPackage.TransferService = request.TransferService;
            existingPackage.PackagePrice = request.PackagePrice;
            existingPackage.StartDate = request.StartDate;
            existingPackage.EndDate = request.EndDate;
            existingPackage.DurationDays = request.DurationDays;
            existingPackage.PackageStatus = (int)request.PackageStatus;

            _context.TravelPackages.Update(existingPackage);
            await _context.SaveChangesAsync();

            return await GetTravelPackageAsync(id);
        }

        public async Task<bool> DeleteTravelPackageAsync(long id)
        {
            var existingPackage = await _context.TravelPackages.FirstOrDefaultAsync(tp => tp.Id == id && tp.DeletedAt == null);
            if (existingPackage == null) return false;

            // Soft delete
            existingPackage.DeletedAt = DateTime.UtcNow;

            _context.TravelPackages.Update(existingPackage);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
