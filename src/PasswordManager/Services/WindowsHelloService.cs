using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;

namespace PasswordManager.Services;

public class WindowsHelloService : IWindowsHelloService
{
    private const string CredentialResourceName = "PasswordManager_MasterKey";
    private readonly PasswordVault _vault = new();

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            return availability == UserConsentVerifierAvailability.Available;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AuthenticateAsync(string message)
    {
        try
        {
            var result = await UserConsentVerifier.RequestVerificationAsync(message);
            return result == UserConsentVerificationResult.Verified;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync()
    {
        return await AuthenticateAsync("Настройка Windows Hello для Password Manager");
    }

    public Task<string?> GetStoredPasswordAsync()
    {
        try
        {
            var credentials = _vault.FindAllByResource(CredentialResourceName);
            if (credentials.Count > 0)
            {
                var credential = credentials[0];
                credential.RetrievePassword();
                
                // Decrypt password using DPAPI
                var encryptedBytes = Convert.FromBase64String(credential.Password);
                var decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    DataProtectionScope.CurrentUser);
                
                return Task.FromResult<string?>(Encoding.UTF8.GetString(decryptedBytes));
            }
        }
        catch
        {
            // Credential not found or error
        }
        
        return Task.FromResult<string?>(null);
    }

    public Task StorePasswordAsync(string password)
    {
        try
        {
            // Remove existing credential first
            RemoveStoredPasswordAsync().Wait();

            // Encrypt password using DPAPI
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var encryptedBytes = ProtectedData.Protect(
                passwordBytes,
                null,
                DataProtectionScope.CurrentUser);
            
            var encryptedPassword = Convert.ToBase64String(encryptedBytes);

            var credential = new PasswordCredential(
                CredentialResourceName,
                "MasterPassword",
                encryptedPassword);
            
            _vault.Add(credential);
        }
        catch
        {
            // Handle error
        }
        
        return Task.CompletedTask;
    }

    public Task RemoveStoredPasswordAsync()
    {
        try
        {
            var credentials = _vault.FindAllByResource(CredentialResourceName);
            foreach (var credential in credentials)
            {
                _vault.Remove(credential);
            }
        }
        catch
        {
            // Credentials not found
        }
        
        return Task.CompletedTask;
    }
}
