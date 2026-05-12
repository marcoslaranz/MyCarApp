using System.Net.Http.Json;
using System.Net.Http.Headers;
using MyCarApp.Client.Models;

namespace MyCarApp.Client.Services;

public class ServiceLogService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ServiceLogService(HttpClient http, AuthService auth)
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

    public async Task<List<ServiceLog>> GetServiceLogsAsync(int vehicleId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ServiceLog>>(
            $"api/vehicles/{vehicleId}/servicelogs") ?? new();
    }

    public async Task<bool> CreateServiceLogAsync(int vehicleId, ServiceLog log, List<ServiceLogItem> items)
    {
        await SetAuthHeader();
        var dto = new
        {
            serviceDate = log.ServiceDate.ToString("yyyy-MM-ddTHH:mm:ss"),
            odometerKm = log.OdometerKm,
            notes = log.Notes,
            items = items.Select(i => new { serviceItemId = i.ServiceItemId, done = i.Done })
        };
        var response = await _http.PostAsJsonAsync($"api/vehicles/{vehicleId}/servicelogs", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteServiceLogAsync(int vehicleId, int id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"api/vehicles/{vehicleId}/servicelogs/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<ServiceDocument?> UploadDocumentAsync(int vehicleId, int serviceLogId, Stream fileStream, string fileName, string contentType)
    {
        await SetAuthHeader();

        // Read stream into byte array first
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var content = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(bytes);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(byteContent, "file", fileName);

        var response = await _http.PostAsync(
            $"api/vehicles/{vehicleId}/servicelogs/{serviceLogId}/documents", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Upload failed: {response.StatusCode} - {error}");
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ServiceDocument>();
    }

    public async Task<bool> DeleteDocumentAsync(int vehicleId, int serviceLogId, int docId)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync(
            $"api/vehicles/{vehicleId}/servicelogs/{serviceLogId}/documents/{docId}");
        return response.IsSuccessStatusCode;
    }
}