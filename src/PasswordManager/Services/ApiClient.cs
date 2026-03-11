using System.Net.Http;
using System.Net.Http.Json;
using PasswordManager.Contracts;
using PasswordManager.Core.Models;

namespace PasswordManager.Services;

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

    public async Task<IReadOnlyList<Credential>> GetCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _http.GetFromJsonAsync<List<Credential>>("/api/credentials", cancellationToken);
        return result ?? [];
    }
}

