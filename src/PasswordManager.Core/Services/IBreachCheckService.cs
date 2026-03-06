namespace PasswordManager.Core.Services;

public interface IBreachCheckService
{
    Task<BreachCheckResult> CheckPasswordAsync(string password);
    Task CheckAllCredentialsAsync(IProgress<BreachCheckProgress>? progress = null);
}

public class BreachCheckResult
{
    public bool IsBreached { get; set; }
    public int BreachCount { get; set; }
}

public class BreachCheckProgress
{
    public int Current { get; set; }
    public int Total { get; set; }
    public string CurrentTitle { get; set; } = string.Empty;
    public int BreachedCount { get; set; }
}
