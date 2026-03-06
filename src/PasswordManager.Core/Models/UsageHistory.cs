using SQLite;

namespace PasswordManager.Core.Models;

public class UsageHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public int CredentialId { get; set; }
    
    [MaxLength(256)]
    public string ProcessName { get; set; } = string.Empty;
    
    [MaxLength(512)]
    public string? WindowTitle { get; set; }
    
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    
    public UsageType Type { get; set; }
}

public enum UsageType
{
    CopyUsername,
    CopyPassword,
    AutoFill
}
