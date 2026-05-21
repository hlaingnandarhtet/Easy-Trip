using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MudBlazor.Services;
using DotNet8.EasyTrip.App.Client.Services;

namespace DotNet8.EasyTrip.App.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Register HttpClient pointing to the Backend Web API port
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7039/") });

            // Register MudBlazor Services
            builder.Services.AddMudServices();
            builder.Services.AddScoped<TravelPackageApiService>();

            await builder.Build().RunAsync();
        }
    }
}

