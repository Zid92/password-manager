using PasswordManager.Models;

namespace PasswordManager.Services;

public interface IRankingService
{
    Task<List<Credential>> GetRankedCredentialsAsync(string processName, string? windowTitle);
    Task RecordUsageAsync(int credentialId, string processName, string? windowTitle, UsageType usageType);
}
