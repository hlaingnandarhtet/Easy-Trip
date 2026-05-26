using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotNet8.EasyTripBackendApi.DbService;
using DotNet8.EasyTripBackendApi.DbService.Models;

class Program
{
    static async Task Main(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var raw = Environment.GetEnvironmentVariable("EASYTRIP_CONNECTION_STRING")
            ?? "Host=db.zjjmggyrlhgbdyormcup.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=2612000@#$!!$;Ssl Mode=Require;Trust Server Certificate=true;";
        optionsBuilder.UseNpgsql(DatabaseConnection.Resolve(raw));

        using var db = new AppDbContext(optionsBuilder.Options);
        
        try
        {
            var sql = @"
            ALTER TABLE buses ADD COLUMN IF NOT EXISTS departure character varying(100) NULL;
            ALTER TABLE buses ADD COLUMN IF NOT EXISTS arrival character varying(100) NULL;
            ALTER TABLE bookings ADD COLUMN IF NOT EXISTS is_used boolean NOT NULL DEFAULT false;
            ";
            
            await db.Database.ExecuteSqlRawAsync(sql);
            Console.WriteLine("Migration completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during migration: " + ex.Message);
        }
    }
}
