namespace PasswordManager.Services;

public interface IEncryptionService
{
    bool IsInitialized { get; }
    void Initialize(string masterPassword);
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string HashPassword(string password, byte[] salt);
    byte[] GenerateSalt();
    bool VerifyPassword(string password, string hash, byte[] salt);
    void Clear();
}
