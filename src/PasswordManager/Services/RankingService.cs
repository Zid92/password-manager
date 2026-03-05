using PasswordManager.Data;
using PasswordManager.Models;

namespace PasswordManager.Services;

public class RankingService : IRankingService
{
    private readonly IDatabaseService _databaseService;
    private readonly ICredentialService _credentialService;

    public RankingService(IDatabaseService databaseService, ICredentialService credentialService)
    {
        _databaseService = databaseService;
        _credentialService = credentialService;
    }

    public async Task<List<Credential>> GetRankedCredentialsAsync(string processName, string? windowTitle)
    {
        var allCredentials = await _credentialService.GetAllAsync();
        var usageHistory = await _databaseService.GetUsageHistoryForContextAsync(processName, windowTitle);

        var scoredCredentials = new List<(Credential credential, double score)>();

        foreach (var credential in allCredentials)
        {
            var score = CalculateScore(credential, usageHistory, processName, windowTitle);
            scoredCredentials.Add((credential, score));
        }

        return scoredCredentials
            .OrderByDescending(x => x.score)
            .Select(x => x.credential)
            .ToList();
    }

    private double CalculateScore(Credential credential, List<UsageHistory> history, string processName, string? windowTitle)
    {
        double score = 0;

        var credentialHistory = history.Where(h => h.CredentialId == credential.Id).ToList();

        if (!credentialHistory.Any())
        {
            // No history - use URL matching if available
            if (!string.IsNullOrEmpty(credential.Url) && !string.IsNullOrEmpty(windowTitle))
            {
                if (ContainsDomain(windowTitle, credential.Url))
                {
                    score += 50;
                }
            }
            return score;
        }

        // Exact match: same process name
        var processMatches = credentialHistory.Where(h => 
            h.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)).ToList();

        if (processMatches.Any())
        {
            score += 30 * Math.Min(processMatches.Count, 5); // Max 150 points for process match frequency

            // Window title similarity bonus
            if (!string.IsNullOrEmpty(windowTitle))
            {
                var titleMatches = processMatches.Where(h => 
                    !string.IsNullOrEmpty(h.WindowTitle) && 
                    (h.WindowTitle.Contains(windowTitle, StringComparison.OrdinalIgnoreCase) ||
                     windowTitle.Contains(h.WindowTitle, StringComparison.OrdinalIgnoreCase))).ToList();

                if (titleMatches.Any())
                {
                    score += 50 * Math.Min(titleMatches.Count, 3); // Max 150 points for title match
                }
            }

            // Recency bonus
            var mostRecent = processMatches.Max(h => h.UsedAt);
            var daysSinceUse = (DateTime.UtcNow - mostRecent).TotalDays;
            
            if (daysSinceUse < 1) score += 100;
            else if (daysSinceUse < 7) score += 50;
            else if (daysSinceUse < 30) score += 20;
        }

        // Overall usage frequency
        score += Math.Min(credentialHistory.Count, 10) * 5;

        return score;
    }

    private bool ContainsDomain(string text, string url)
    {
        try
        {
            var uri = new Uri(url);
            var domain = uri.Host.Replace("www.", "");
            return text.Contains(domain, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public async Task RecordUsageAsync(int credentialId, string processName, string? windowTitle, UsageType usageType)
    {
        var history = new UsageHistory
        {
            CredentialId = credentialId,
            ProcessName = processName,
            WindowTitle = windowTitle,
            UsedAt = DateTime.UtcNow,
            Type = usageType
        };

        await _databaseService.AddUsageHistoryAsync(history);
    }
}
