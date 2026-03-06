using PasswordManager.Core.Services;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace PasswordManager.Maui.Services;

public class MauiBiometricService : IBiometricService
{
    private const string MasterKeyStorageKey = "PasswordManager_MasterKey";
    private const string BiometricEnabledKey = "PasswordManager_BiometricEnabled";

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await CrossFingerprint.Current.IsAvailableAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
        try
        {
            var request = new AuthenticationRequestConfiguration("Password Manager", reason)
            {
                CancelTitle = "Cancel",
                FallbackTitle = "Use Password",
                AllowAlternativeAuthentication = true
            };

            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            return result.Authenticated;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsEnabledAsync()
    {
        try
        {
            var value = await SecureStorage.Default.GetAsync(BiometricEnabledKey);
            return value == "true";
        }
        catch
        {
            return false;
        }
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        try
        {
            if (enabled)
            {
                await SecureStorage.Default.SetAsync(BiometricEnabledKey, "true");
            }
            else
            {
                SecureStorage.Default.Remove(BiometricEnabledKey);
            }
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public async Task StoreMasterKeyAsync(string key)
    {
        try
        {
            await SecureStorage.Default.SetAsync(MasterKeyStorageKey, key);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public async Task<string?> RetrieveMasterKeyAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(MasterKeyStorageKey);
        }
        catch
        {
            return null;
        }
    }

    public Task ClearStoredKeyAsync()
    {
        try
        {
            SecureStorage.Default.Remove(MasterKeyStorageKey);
            SecureStorage.Default.Remove(BiometricEnabledKey);
        }
        catch
        {
            // Ignore storage errors
        }
        return Task.CompletedTask;
    }
}
