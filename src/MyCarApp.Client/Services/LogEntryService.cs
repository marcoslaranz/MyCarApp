using System.Net.Http.Json;
using System.Net.Http.Headers;
using MyCarApp.Client.Models;

namespace MyCarApp.Client.Services;

public class LogEntryService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public LogEntryService(HttpClient http, AuthService auth)
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

    public async Task<List<LogEntry>> GetLogsAsync(int vehicleId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<LogEntry>>($"api/vehicles/{vehicleId}/logs") ?? new();
    }

    public async Task<bool> CreateLogAsync(int vehicleId, LogEntry log)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync($"api/vehicles/{vehicleId}/logs", log);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateLogAsync(int vehicleId, LogEntry log)
    {
        await SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"api/vehicles/{vehicleId}/logs/{log.Id}", log);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteLogAsync(int vehicleId, int logId)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vehicles/{vehicleId}/logs/{logId}");
        return response.IsSuccessStatusCode;
    }
}