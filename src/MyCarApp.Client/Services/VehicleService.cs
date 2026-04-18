using System.Net.Http.Json;
using System.Net.Http.Headers;
using MyCarApp.Client.Models;

namespace MyCarApp.Client.Services;

public class VehicleService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public VehicleService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    private async Task SetAuthHeader()
    {
        var token = await _auth.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<Vehicle>> GetVehiclesAsync()
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<Vehicle>>("api/vehicles") ?? new();
    }

    public async Task<bool> CreateVehicleAsync(Vehicle vehicle)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("api/vehicles", vehicle);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateVehicleAsync(Vehicle vehicle)
    {
        await SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/vehicles/{vehicle.Id}", vehicle);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteVehicleAsync(int id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vehicles/{id}");
        return response.IsSuccessStatusCode;
    }
}