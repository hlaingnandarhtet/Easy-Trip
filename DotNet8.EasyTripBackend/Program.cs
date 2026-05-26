
using Microsoft.EntityFrameworkCore;
using DotNet8.EasyTripBackendApi.DbService;
using DotNet8.EasyTripBackend.Features.Hotels;
using DotNet8.EasyTripBackend.Features.HotelRooms;
using DotNet8.EasyTripBackend.Features.RoomTypes;
using DotNet8.EasyTripBackend.Features.TravelPackages;
using DotNet8.EasyTripBackend.Features.Bookings;
using DotNet8.EasyTripBackend.Features.Reports;

namespace DotNet8.EasyTripBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddAuthorization();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorApp", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            
            // Register DbContext and Services (pooler host avoids IPv6-only DNS issues on Windows)
            var pooler = builder.Configuration.GetConnectionString("PoolerConnection");
            var connectionString = Environment.GetEnvironmentVariable("EASYTRIP_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
                connectionString = !string.IsNullOrWhiteSpace(pooler)
                    ? pooler
                    : builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Database connection is not configured. Set ConnectionStrings:PoolerConnection in appsettings.json (Session pooler URI from Supabase Dashboard), or EASYTRIP_CONNECTION_STRING.");

            connectionString = DatabaseConnection.Resolve(connectionString);
            builder.Services.AddDbContext<DotNet8.EasyTripBackendApi.DbService.Models.AppDbContext>(options =>
                options.UseNpgsql(connectionString));
            builder.Services.AddScoped<DotNet8.EasyTripBackend.Features.Bus.IBusService, DotNet8.EasyTripBackend.Features.Bus.BusService>();
            builder.Services.AddScoped<DotNet8.EasyTripBackend.Features.Hotels.IHotelService, DotNet8.EasyTripBackend.Features.Hotels.HotelService>();
            builder.Services.AddScoped<IHotelRoomService, HotelRoomService>();
            builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
            builder.Services.AddScoped<ITravelPackageService, TravelPackageService>();
            builder.Services.AddScoped<DotNet8.EasyTripBackend.Features.BusTypes.IBusTypeService, DotNet8.EasyTripBackend.Features.BusTypes.BusTypeService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<DotNet8.EasyTripBackend.Features.Payments.IPaymentService, DotNet8.EasyTripBackend.Features.Payments.PaymentService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowBlazorApp");

            app.UseAuthorization();
            app.MapControllers();

            //var summaries = new[]
            //{
            //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            //};

            //app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            //{
            //    var forecast = Enumerable.Range(1, 5).Select(index =>
            //        new WeatherForecast
            //        {
            //            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            //            TemperatureC = Random.Shared.Next(-20, 55),
            //            Summary = summaries[Random.Shared.Next(summaries.Length)]
            //        })
            //        .ToArray();
            //    return forecast;
            //})
            //.WithName("GetWeatherForecast")
            //.WithOpenApi();

            // Automatically seed database lookup tables on startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<DotNet8.EasyTripBackendApi.DbService.Models.AppDbContext>();
                    if (!context.Database.CanConnectAsync().GetAwaiter().GetResult())
                        throw new InvalidOperationException("Database.CanConnectAsync returned false.");

                    DbSeeder.SeedAsync(context).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DATABASE CONNECTION FAILED — API requests will return 500.");
                    Console.WriteLine(ex.GetBaseException().Message);
                    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("PoolerConnection")))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Fix: Supabase Dashboard → Project Settings → Database → copy the");
                        Console.WriteLine("     \"Session pooler\" connection string into appsettings.json:");
                        Console.WriteLine("     ConnectionStrings:PoolerConnection");
                    }
                    Console.ResetColor();
                }
            }

            app.Run();
        }
    }
}
