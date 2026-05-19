using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MudBlazor.Services;

namespace DotNet8.EasyTrip.App.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Register HttpClient pointing to the Backend Web API port
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5157/") });

            // Register MudBlazor Services
            builder.Services.AddMudServices();

            await builder.Build().RunAsync();
        }
    }
}

