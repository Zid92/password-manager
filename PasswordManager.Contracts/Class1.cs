namespace PasswordManager.Contracts;

public sealed record OpenVaultRequest(string MasterPassword);

public sealed class OpenVaultResponse
{
    public bool Success { get; set; }
}

public sealed class SaveCredentialRequest
{
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PlainPassword { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }
}

public sealed class BreachCheckRequest
{
    public string Password { get; set; } = string.Empty;
}
