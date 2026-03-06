using PasswordManager.Core.Models;

namespace PasswordManager.Core.Data;

public interface IDatabaseService
{
    Task InitializeAsync(string password);
    Task<bool> IsFirstRunAsync();
    Task<List<Credential>> GetAllCredentialsAsync();
    Task<Credential?> GetCredentialByIdAsync(int id);
    Task<int> SaveCredentialAsync(Credential credential);
    Task<int> DeleteCredentialAsync(int id);
    Task<List<Credential>> SearchCredentialsAsync(string query);
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value);
    Task AddUsageHistoryAsync(UsageHistory history);
    Task<List<UsageHistory>> GetUsageHistoryForContextAsync(string processName, string? windowTitle);
    void Close();
}
