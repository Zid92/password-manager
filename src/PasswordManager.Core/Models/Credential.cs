using SQLite;

namespace PasswordManager.Core.Models;

public class Credential
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(256)]
    public string Username { get; set; } = string.Empty;
    
    public string EncryptedPassword { get; set; } = string.Empty;
    
    [MaxLength(512)]
    public string? Url { get; set; }
    
    public string? Notes { get; set; }
    
    public string? Category { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsBreached { get; set; }
    
    public DateTime? LastBreachCheck { get; set; }
}
