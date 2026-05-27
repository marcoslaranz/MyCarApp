using System.Net.Http.Headers;
using System.Net.Http.Json;
using MyCarApp.Client.Models;

namespace MyCarApp.Client.Services;

public class ImportService
{
    private readonly HttpClient _http;

    public ImportService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ImportBatch?> GetLastImportAsync(int vehicleId)
    {
        try
        {
            return await _http.GetFromJsonAsync<ImportBatch>($"api/import/{vehicleId}/last");
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string Message)> ImportAsync(int vehicleId, Stream fileStream, string fileName)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            content.Add(fileContent, "file", fileName);

            var response = await _http.PostAsync($"api/import/{vehicleId}", content);
            var result = await response.Content.ReadFromJsonAsync<ImportResultDto>();

            if (response.IsSuccessStatusCode)
                return (true, result?.Message ?? "Import complete.");
            else
                return (false, result?.Message ?? "Import failed.");
        }
        catch (Exception ex)
        {
            return (false, $"Import failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UndoLastImportAsync(int vehicleId)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/import/{vehicleId}/last");
            var result = await response.Content.ReadFromJsonAsync<ImportResultDto>();

            if (response.IsSuccessStatusCode)
                return (true, result?.Message ?? "Import reversed.");
            else
                return (false, result?.Message ?? "Undo failed.");
        }
        catch (Exception ex)
        {
            return (false, $"Undo failed: {ex.Message}");
        }
    }

    private class ImportResultDto
    {
        public string Message { get; set; } = string.Empty;
    }
}