using System.Net.Http.Json;
using PasswordManager.Contracts;
using PasswordManager.Core.Models;

namespace PasswordManager.Blazor.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> OpenVaultAsync(string masterPassword, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/vault/open", new OpenVaultRequest(masterPassword), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<OpenVaultResponse>(cancellationToken: cancellationToken);
        return result?.Success == true;
    }

    public async Task LockVaultAsync(CancellationToken cancellationToken = default)
    {
        await _http.PostAsync("/api/vault/lock", content: null, cancellationToken);
    }

    public async Task<IReadOnlyList<Credential>> GetCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _http.GetFromJsonAsync<List<Credential>>("/api/credentials", cancellationToken);
        return result ?? [];
    }

    public async Task<Credential?> GetCredentialAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<Credential>($"/api/credentials/{id}", cancellationToken);
    }

    public async Task<int> CreateCredentialAsync(SaveCredentialRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/credentials", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(cancellationToken: cancellationToken);
        return payload?["Id"] ?? 0;
    }

    public async Task UpdateCredentialAsync(int id, SaveCredentialRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PutAsJsonAsync($"/api/credentials/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCredentialAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync($"/api/credentials/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Credential>> SearchCredentialsAsync(string query, CancellationToken cancellationToken = default)
    {
        var result = await _http.GetFromJsonAsync<List<Credential>>($"/api/credentials/search?q={Uri.EscapeDataString(query)}", cancellationToken);
        return result ?? [];
    }

    public async Task<BreachCheckResponse> CheckPasswordAsync(string password, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/breach-check", new BreachCheckRequest { Password = password }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BreachCheckResponse>(cancellationToken: cancellationToken);
        return result ?? new BreachCheckResponse();
    }
}

public sealed class BreachCheckResponse
{
    public bool IsBreached { get; set; }
    public int BreachCount { get; set; }
}

