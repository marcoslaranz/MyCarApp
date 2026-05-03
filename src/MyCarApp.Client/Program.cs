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
    
    // Use Railway API in production, local API in development
    var apiBase = baseUri.Host.Contains("localhost") || baseUri.Host.Contains("192.168")
        ? new Uri($"{baseUri.Scheme}://{baseUri.Host}:5236/")
        : new Uri("https://mycarapp-production.up.railway.app/");
    
    return new HttpClient { BaseAddress = apiBase };
});


// Add JSON options with custom DateTime converter
//builder.Services.Configure<Microsoft.AspNetCore.Components.WebAssembly.Http.WebAssemblyHttpRequestMessageOptions>(options => { });

//var jsonOptions = new JsonSerializerOptions();
//jsonOptions.Converters.Add(new MyCarApp.Client.Models.UnspecifiedDateTimeConverter());
//builder.Services.AddSingleton(jsonOptions);

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<LogEntryService>();

await builder.Build().RunAsync();
