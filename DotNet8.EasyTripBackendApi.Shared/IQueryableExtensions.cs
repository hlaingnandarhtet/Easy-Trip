using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet8.EasyTripBackendApi.Shared
{
    public static class IQueryableExtensions
    {
        public static async Task<PaginationResponse<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageNo, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNo - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginationResponse<T>(items, count, pageNo, pageSize);
        }
    }
}
