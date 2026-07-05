using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyCarApp.Client;
using MyCarApp.Client.Services;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<NavigationService>();


builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var baseUri = new Uri(navigationManager.BaseUri);

    var apiUrl = baseUri.Host.Contains("localhost") || baseUri.Host.Contains("192.168")
        ? $"{baseUri.Scheme}://{baseUri.Host}:5236/"
        : "https://mycarapp-api.onrender.com/";

    return new HttpClient { BaseAddress = new Uri(apiUrl) };
});


builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<LogEntryService>();
builder.Services.AddScoped<ServiceItemService>();
builder.Services.AddScoped<ServiceLogService>();

await builder.Build().RunAsync();