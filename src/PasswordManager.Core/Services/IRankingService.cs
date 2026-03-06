using PasswordManager.Core.Models;

namespace PasswordManager.Core.Services;

public interface IRankingService
{
    Task<List<Credential>> GetRankedCredentialsAsync(string processName, string? windowTitle);
    Task RecordUsageAsync(int credentialId, string processName, string? windowTitle, UsageType usageType);
}
