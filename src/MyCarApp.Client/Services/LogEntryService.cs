using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

    // Serialize datetime as plain string without timezone
    private string SerializeLog(LogEntry log)
    {
        return JsonSerializer.Serialize(new
        {
            dateTime = log.DateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
            odometerKm = log.OdometerKm,
            fuelLoaded = log.FuelLoaded,
            fuelLiters = log.FuelLiters,
            fuelPricePerLiter = log.FuelPricePerLiter,
            fuelTotalPaid = log.FuelTotalPaid,
            petrolStationName = log.PetrolStationName
        });
    }

    public async Task<List<LogEntry>> GetLogsAsync(int vehicleId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<LogEntry>>($"api/vehicles/{vehicleId}/logs") ?? new();
    }

    public async Task<bool> CreateLogAsync(int vehicleId, LogEntry log)
    {
        await SetAuthHeader();
        var content = new StringContent(SerializeLog(log), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"api/vehicles/{vehicleId}/logs", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CreateLog failed: {response.StatusCode} - {error}");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateLogAsync(int vehicleId, LogEntry log)
    {
        await SetAuthHeader();
        var content = new StringContent(SerializeLog(log), Encoding.UTF8, "application/json");
        var response = await _http.PutAsync($"api/vehicles/{vehicleId}/logs/{log.Id}", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteLogAsync(int vehicleId, int logId)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vehicles/{vehicleId}/logs/{logId}");
        return response.IsSuccessStatusCode;
    }
}