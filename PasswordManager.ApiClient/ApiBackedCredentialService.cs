using System.Net.Http.Json;
using PasswordManager.Contracts;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;

namespace PasswordManager.ApiClient;

/// <summary>
/// <see cref="ICredentialService"/> implementation that uses the central HTTP API.
/// Use when running in "remote" mode so all apps share the same vault on the server.
/// </summary>
public sealed class ApiBackedCredentialService : ICredentialService
{
    private readonly HttpClient _http;

    public ApiBackedCredentialService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Credential>> GetAllAsync()
    {
        var list = await _http.GetFromJsonAsync<List<Credential>>("/api/credentials");
        return list ?? new List<Credential>();
    }

    public async Task<Credential?> GetByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Credential>($"/api/credentials/{id}");
    }

    public async Task<int> SaveAsync(Credential credential, string plainPassword)
    {
        var request = new SaveCredentialRequest
        {
            Title = credential.Title,
            Username = credential.Username,
            PlainPassword = plainPassword,
            Url = credential.Url,
            Notes = credential.Notes,
            Category = credential.Category
        };

        if (credential.Id == 0)
        {
            var response = await _http.PostAsJsonAsync("/api/credentials", request);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<IdResponse>();
            return payload?.Id ?? 0;
        }

        var putResponse = await _http.PutAsJsonAsync($"/api/credentials/{credential.Id}", request);
        putResponse.EnsureSuccessStatusCode();
        return credential.Id;
    }

    public async Task<int> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/credentials/{id}");
        return response.IsSuccessStatusCode ? 1 : 0;
    }

    public async Task<List<Credential>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();
        var list = await _http.GetFromJsonAsync<List<Credential>>(
            $"/api/credentials/search?q={Uri.EscapeDataString(query)}");
        return list ?? new List<Credential>();
    }

    public async Task<string> DecryptPasswordAsync(Credential credential)
    {
        var response = await _http.GetAsync($"/api/credentials/{credential.Id}/password");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PasswordResponse>();
        return payload?.Password ?? string.Empty;
    }

    public string DecryptPassword(Credential credential)
    {
        return DecryptPasswordAsync(credential).GetAwaiter().GetResult();
    }

    private sealed class IdResponse
    {
        public int Id { get; set; }
    }

    private sealed class PasswordResponse
    {
        public string Password { get; set; } = string.Empty;
    }
}
