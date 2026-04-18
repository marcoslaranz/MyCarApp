using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace MyCarApp.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<(bool Success, string? Error)> Register(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new { email, password });

        if (response.IsSuccessStatusCode)
            return (true, null);

        var content = await response.Content.ReadAsStringAsync();
        return (false, content);
    }

    public async Task<bool> Login(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = result.GetProperty("token").GetString();

        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        return true;
    }

    public async Task Logout()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", "authToken");
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}