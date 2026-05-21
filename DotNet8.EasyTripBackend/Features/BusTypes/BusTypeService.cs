using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService.Models;
using DotNet8.EasyTripBackendApi.Models;

namespace DotNet8.EasyTripBackend.Features.BusTypes;

public class BusTypeService : IBusTypeService
{
    private readonly AppDbContext _context;

    public BusTypeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusTypeResponseModel>> GetBusTypesAsync()
    {
        return await _context.BusTypes
            .AsNoTracking()
            .Select(bt => new BusTypeResponseModel
            {
                Id = bt.Id,
                TypeName = bt.TypeName
            }).ToListAsync();
    }

    public async Task<BusTypeResponseModel?> GetBusTypeByIdAsync(long id)
    {
        var bt = await _context.BusTypes.FindAsync(id);
        if (bt == null) return null;

        return new BusTypeResponseModel
        {
            Id = bt.Id,
            TypeName = bt.TypeName
        };
    }

    public async Task<BusTypeResponseModel> CreateBusTypeAsync(BusTypeRequestModel request)
    {
        var busType = new BusType
        {
            TypeName = request.TypeName
        };

        _context.BusTypes.Add(busType);
        await _context.SaveChangesAsync();

        return new BusTypeResponseModel
        {
            Id = busType.Id,
            TypeName = busType.TypeName
        };
    }

    public async Task<bool> UpdateBusTypeAsync(long id, BusTypeRequestModel request)
    {
        var bt = await _context.BusTypes.FindAsync(id);
        if (bt == null) return false;

        bt.TypeName = request.TypeName;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBusTypeAsync(long id)
    {
        var bt = await _context.BusTypes.FindAsync(id);
        if (bt == null) return false;

        _context.BusTypes.Remove(bt);
        await _context.SaveChangesAsync();
        return true;
    }
}
