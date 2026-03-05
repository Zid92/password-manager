using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services;

public class EncryptionService : IEncryptionService
{
    private byte[]? _key;
    private const int KeySize = 32; // 256 bits for AES-256
    private const int SaltSize = 32;
    private const int Iterations = 100000;

    public void Initialize(string masterPassword)
    {
        _key = Rfc2898DeriveBytes.Pbkdf2(
            masterPassword,
            Encoding.UTF8.GetBytes("PasswordManagerFixedSalt"),
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }

    public string Encrypt(string plainText)
    {
        if (_key == null)
            throw new InvalidOperationException("Encryption service not initialized");

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (_key == null)
            throw new InvalidOperationException("Encryption service not initialized");

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(SaltSize);
    }

    public string HashPassword(string password, byte[] salt)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string hash, byte[] salt)
    {
        var newHash = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(hash),
            Convert.FromBase64String(newHash));
    }

    public void Clear()
    {
        if (_key != null)
        {
            CryptographicOperations.ZeroMemory(_key);
            _key = null;
        }
    }
}
