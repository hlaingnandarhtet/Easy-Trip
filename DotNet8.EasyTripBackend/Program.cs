
using DotNet8.EasyTripBackend.Features.Hotels;
using DotNet8.EasyTripBackend.Features.HotelRooms;
using DotNet8.EasyTripBackend.Features.RoomTypes;
using DotNet8.EasyTripBackend.Features.TravelPackages;
using DotNet8.EasyTripBackend.Features.Bookings;

namespace DotNet8.EasyTripBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddAuthorization();
            
            // Register DbContext and Services
            builder.Services.AddDbContext<DotNet8.EasyTripBackendApi.DbService.Models.AppDbContext>();
            builder.Services.AddScoped<DotNet8.EasyTripBackend.Features.Bus.IBusService, DotNet8.EasyTripBackend.Features.Bus.BusService>();
            builder.Services.AddScoped<DotNet8.EasyTripBackend.Features.Hotels.IHotelService, DotNet8.EasyTripBackend.Features.Hotels.HotelService>();
            builder.Services.AddScoped<IHotelRoomService, HotelRoomService>();
            builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
            builder.Services.AddScoped<ITravelPackageService, TravelPackageService>();
            builder.Services.AddScoped<IBookingService, BookingService>();

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
                    DbSeeder.SeedAsync(context).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding lookup database tables: {ex.Message}");
                }
            }

            app.Run();
        }
    }
}
