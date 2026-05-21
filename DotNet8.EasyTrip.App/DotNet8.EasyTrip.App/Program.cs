using System.Net.Http;
using DotNet8.EasyTrip.App.Client.Pages;
using DotNet8.EasyTrip.App.Components;
using MudBlazor.Services;

namespace DotNet8.EasyTrip.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddMudServices();
            builder.Services.AddScoped<DotNet8.EasyTrip.App.Client.Services.DrawerStateService>();
            builder.Services.AddScoped<DotNet8.EasyTrip.App.Client.Services.TravelPackageApiService>();

            // Register HttpClient pointing to the Backend Web API port for Server Prerendering
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7039/") });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            //app.MapRazorComponents<App>()
            app.MapRazorComponents<global::DotNet8.EasyTrip.App.Components.App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
