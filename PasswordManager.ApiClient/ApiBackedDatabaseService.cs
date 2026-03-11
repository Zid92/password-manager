using System.Net.Http.Json;
using PasswordManager.Contracts;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;

namespace PasswordManager.ApiClient;

/// <summary>
/// <see cref="IDatabaseService"/> implementation that delegates to the central HTTP API.
/// Use together with <see cref="ApiBackedCredentialService"/> when running in "remote" mode.
/// </summary>
public sealed class ApiBackedDatabaseService : IDatabaseService
{
    private readonly HttpClient _http;

    public ApiBackedDatabaseService(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync(string password)
    {
        var response = await _http.PostAsJsonAsync("/api/vault/open", new OpenVaultRequest(password));
        response.EnsureSuccessStatusCode();
    }

    public Task<bool> IsFirstRunAsync()
    {
        // Remote vault is assumed to exist (create via Blazor or first run on server).
        return Task.FromResult(false);
    }

    public async Task<List<Credential>> GetAllCredentialsAsync()
    {
        var list = await _http.GetFromJsonAsync<List<Credential>>("/api/credentials");
        return list ?? new List<Credential>();
    }

    public async Task<Credential?> GetCredentialByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Credential>($"/api/credentials/{id}");
    }

    public Task<int> SaveCredentialAsync(Credential credential)
    {
        // Saving with plain password is done via ICredentialService.SaveAsync → API.
        throw new NotSupportedException("Use ICredentialService.SaveAsync when using remote API.");
    }

    public async Task<int> DeleteCredentialAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/credentials/{id}");
        return response.IsSuccessStatusCode ? 1 : 0;
    }

    public async Task<List<Credential>> SearchCredentialsAsync(string query)
    {
        var list = await _http.GetFromJsonAsync<List<Credential>>(
            $"/api/credentials/search?q={Uri.EscapeDataString(query ?? "")}");
        return list ?? new List<Credential>();
    }

    public Task<string?> GetSettingAsync(string key)
    {
        return Task.FromResult<string?>(null);
    }

    public Task SetSettingAsync(string key, string value)
    {
        return Task.CompletedTask;
    }

    public Task AddUsageHistoryAsync(UsageHistory history)
    {
        return Task.CompletedTask;
    }

    public Task<List<UsageHistory>> GetUsageHistoryForContextAsync(string processName, string? windowTitle)
    {
        return Task.FromResult(new List<UsageHistory>());
    }

    public async void Close()
    {
        try
        {
            await _http.PostAsync("/api/vault/lock", null);
        }
        catch
        {
            // Best effort
        }
    }
}
