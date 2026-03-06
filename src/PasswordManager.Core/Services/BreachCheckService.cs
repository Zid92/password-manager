using System.Security.Cryptography;
using System.Text;
using PasswordManager.Core.Data;

namespace PasswordManager.Core.Services;

public class BreachCheckService : IBreachCheckService
{
    private readonly HttpClient _httpClient;
    private readonly IDatabaseService _databaseService;
    private readonly ICredentialService _credentialService;

    public BreachCheckService(
        IDatabaseService databaseService,
        ICredentialService credentialService)
    {
        _databaseService = databaseService;
        _credentialService = credentialService;
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.pwnedpasswords.com/")
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PasswordManager-App");
    }

    public async Task<BreachCheckResult> CheckPasswordAsync(string password)
    {
        var sha1Hash = ComputeSha1Hash(password);
        var prefix = sha1Hash[..5];
        var suffix = sha1Hash[5..];

        try
        {
            var response = await _httpClient.GetStringAsync($"range/{prefix}");
            
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    var hashSuffix = parts[0].Trim();
                    if (hashSuffix.Equals(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(parts[1].Trim(), out int count))
                        {
                            return new BreachCheckResult
                            {
                                IsBreached = true,
                                BreachCount = count
                            };
                        }
                    }
                }
            }

            return new BreachCheckResult { IsBreached = false, BreachCount = 0 };
        }
        catch (Exception)
        {
            return new BreachCheckResult { IsBreached = false, BreachCount = -1 };
        }
    }

    public async Task CheckAllCredentialsAsync(IProgress<BreachCheckProgress>? progress = null)
    {
        var credentials = await _credentialService.GetAllAsync();
        var total = credentials.Count;
        var current = 0;
        var breachedCount = 0;

        foreach (var credential in credentials)
        {
            current++;

            var password = _credentialService.DecryptPassword(credential);
            var result = await CheckPasswordAsync(password);

            credential.IsBreached = result.IsBreached;
            credential.LastBreachCheck = DateTime.UtcNow;
            
            await _databaseService.SaveCredentialAsync(credential);

            if (result.IsBreached)
            {
                breachedCount++;
            }

            progress?.Report(new BreachCheckProgress
            {
                Current = current,
                Total = total,
                CurrentTitle = credential.Title,
                BreachedCount = breachedCount
            });

            await Task.Delay(1600);
        }
    }

    internal static string ComputeSha1Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA1.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
