namespace PasswordManager.Core.Services;

public interface IBiometricService
{
    Task<bool> IsAvailableAsync();
    Task<bool> AuthenticateAsync(string reason);
    Task<bool> IsEnabledAsync();
    Task SetEnabledAsync(bool enabled);
    Task StoreMasterKeyAsync(string key);
    Task<string?> RetrieveMasterKeyAsync();
    Task ClearStoredKeyAsync();
}

public enum BiometricType
{
    None,
    Fingerprint,
    FaceRecognition,
    Iris,
    WindowsHello
}
