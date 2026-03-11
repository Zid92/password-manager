using PasswordManager.Core.Services;

namespace PasswordManager.ApiClient;

/// <summary>
/// Stub for remote mode: vault and decryption live on the server; local encryption is not used.
/// </summary>
public sealed class RemoteStubEncryptionService : IEncryptionService
{
    public bool IsInitialized => true;

    public void Initialize(string masterPassword) { }

    public void Clear() { }

    public string Encrypt(string plainText) =>
        throw new NotSupportedException("Encryption is performed on the server when using remote API.");

    public string Decrypt(string cipherText) =>
        throw new NotSupportedException("Decryption is performed on the server when using remote API.");

    public byte[] GenerateSalt() =>
        throw new NotSupportedException("Not used when using remote API.");

    public string HashPassword(string password, byte[] salt) =>
        throw new NotSupportedException("Not used when using remote API.");

    public bool VerifyPassword(string password, string hash, byte[] salt) =>
        throw new NotSupportedException("Password verification is done by the server when using remote API.");
}
