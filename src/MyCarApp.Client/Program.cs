using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyCarApp.Client;
using MyCarApp.Client.Services;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

/*
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://192.168.20.15:5236/")
});
*/

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var baseUri = new Uri(navigationManager.BaseUri);
    var apiBase = new Uri($"{baseUri.Scheme}://{baseUri.Host}:5236/");
    
    return new HttpClient { BaseAddress = apiBase };
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<LogEntryService>();

await builder.Build().RunAsync();