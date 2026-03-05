namespace PasswordManager.Services;

public interface IWindowsHelloService
{
    Task<bool> IsAvailableAsync();
    Task<bool> AuthenticateAsync(string message);
    Task<bool> RegisterAsync();
    Task<string?> GetStoredPasswordAsync();
    Task StorePasswordAsync(string password);
    Task RemoveStoredPasswordAsync();
}
