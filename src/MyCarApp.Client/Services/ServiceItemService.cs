using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MyCarApp.Client.Models;

namespace MyCarApp.Client.Services;

public class ServiceItemService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ServiceItemService(HttpClient http, AuthService auth)
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

    public async Task<List<ServiceItem>> GetServiceItemsAsync(int vehicleId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ServiceItem>>(
            $"api/vehicles/{vehicleId}/serviceitems") ?? new();
    }

    public async Task<bool> CreateServiceItemAsync(int vehicleId, ServiceItem item)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync(
            $"api/vehicles/{vehicleId}/serviceitems", item);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateServiceItemAsync(int vehicleId, ServiceItem item)
    {
        await SetAuthHeader();
        var response = await _http.PutAsJsonAsync(
            $"api/vehicles/{vehicleId}/serviceitems/{item.Id}", item);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteServiceItemAsync(int vehicleId, int id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync(
            $"api/vehicles/{vehicleId}/serviceitems/{id}");
        return response.IsSuccessStatusCode;
    }
}