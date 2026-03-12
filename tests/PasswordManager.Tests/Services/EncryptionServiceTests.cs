using PasswordManager.Core.Services;

namespace PasswordManager.Tests.Services;

public class EncryptionServiceTests
{
    private readonly EncryptionService _sut;

    public EncryptionServiceTests()
    {
        _sut = new EncryptionService();
    }

    [Fact]
    public void IsInitialized_WhenNotInitialized_ReturnsFalse()
    {
        Assert.False(_sut.IsInitialized);
    }

    [Fact]
    public void Initialize_WithPassword_SetsIsInitializedTrue()
    {
        _sut.Initialize("TestPassword123");

        Assert.True(_sut.IsInitialized);
    }

    [Fact]
    public void Encrypt_WhenNotInitialized_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => _sut.Encrypt("test"));

        Assert.Contains("not initialized", exception.Message);
    }

    [Fact]
    public void Decrypt_WhenNotInitialized_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => _sut.Decrypt("test"));

        Assert.Contains("not initialized", exception.Message);
    }

    [Fact]
    public void Encrypt_WithValidInput_ReturnsBase64String()
    {
        _sut.Initialize("TestPassword123");
        var plainText = "MySecretPassword";

        var encrypted = _sut.Encrypt(plainText);

        Assert.NotNull(encrypted);
        Assert.NotEmpty(encrypted);
        Assert.NotEqual(plainText, encrypted);
        Assert.True(IsBase64String(encrypted));
    }

    [Fact]
    public void Decrypt_WithValidCipherText_ReturnsOriginalText()
    {
        _sut.Initialize("TestPassword123");
        var originalText = "MySecretPassword";
        var encrypted = _sut.Encrypt(originalText);

        var decrypted = _sut.Decrypt(encrypted);

        Assert.Equal(originalText, decrypted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("short")]
    [InlineData("This is a longer password with spaces and special chars !@#$%")]
    [InlineData("パスワード日本語")] // password text in Japanese
    [InlineData("密码中文")]
    [InlineData("🔐🔑💻")]
    public void EncryptDecrypt_WithVariousInputs_ReturnsOriginal(string input)
    {
        _sut.Initialize("TestPassword123");

        var encrypted = _sut.Encrypt(input);
        var decrypted = _sut.Decrypt(encrypted);

        Assert.Equal(input, decrypted);
    }

    [Fact]
    public void Encrypt_SameInputTwice_ReturnsDifferentCipherText()
    {
        _sut.Initialize("TestPassword123");
        var plainText = "MySecretPassword";

        var encrypted1 = _sut.Encrypt(plainText);
        var encrypted2 = _sut.Encrypt(plainText);

        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void Decrypt_WithDifferentPassword_ThrowsException()
    {
        _sut.Initialize("Password1");
        var encrypted = _sut.Encrypt("SecretData");

        var service2 = new EncryptionService();
        service2.Initialize("Password2");

        Assert.ThrowsAny<Exception>(() => service2.Decrypt(encrypted));
    }

    [Fact]
    public void GenerateSalt_ReturnsSaltOfCorrectLength()
    {
        var salt = _sut.GenerateSalt();

        Assert.NotNull(salt);
        Assert.Equal(32, salt.Length);
    }

    [Fact]
    public void GenerateSalt_CalledTwice_ReturnsDifferentSalts()
    {
        var salt1 = _sut.GenerateSalt();
        var salt2 = _sut.GenerateSalt();

        Assert.False(salt1.SequenceEqual(salt2));
    }

    [Fact]
    public void HashPassword_WithSameSalt_ReturnsSameHash()
    {
        var password = "TestPassword";
        var salt = _sut.GenerateSalt();

        var hash1 = _sut.HashPassword(password, salt);
        var hash2 = _sut.HashPassword(password, salt);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_WithDifferentSalts_ReturnsDifferentHashes()
    {
        var password = "TestPassword";
        var salt1 = _sut.GenerateSalt();
        var salt2 = _sut.GenerateSalt();

        var hash1 = _sut.HashPassword(password, salt1);
        var hash2 = _sut.HashPassword(password, salt2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var password = "TestPassword";
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword(password, salt);

        var result = _sut.VerifyPassword(password, hash, salt);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ReturnsFalse()
    {
        var password = "TestPassword";
        var wrongPassword = "WrongPassword";
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword(password, salt);

        var result = _sut.VerifyPassword(wrongPassword, hash, salt);

        Assert.False(result);
    }

    [Fact]
    public void Clear_AfterInitialize_SetsIsInitializedFalse()
    {
        _sut.Initialize("TestPassword");
        Assert.True(_sut.IsInitialized);

        _sut.Clear();

        Assert.False(_sut.IsInitialized);
    }

    [Fact]
    public void Clear_ThenEncrypt_ThrowsException()
    {
        _sut.Initialize("TestPassword");
        _sut.Clear();

        Assert.Throws<InvalidOperationException>(() => _sut.Encrypt("test"));
    }

    private static bool IsBase64String(string s)
    {
        try
        {
            Convert.FromBase64String(s);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
