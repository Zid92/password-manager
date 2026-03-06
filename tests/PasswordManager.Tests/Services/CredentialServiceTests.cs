using Moq;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;

namespace PasswordManager.Tests.Services;

public class CredentialServiceTests
{
    private readonly Mock<IDatabaseService> _mockDatabase;
    private readonly Mock<IEncryptionService> _mockEncryption;
    private readonly CredentialService _sut;

    public CredentialServiceTests()
    {
        _mockDatabase = new Mock<IDatabaseService>();
        _mockEncryption = new Mock<IEncryptionService>();
        _sut = new CredentialService(_mockDatabase.Object, _mockEncryption.Object);
    }

    [Fact]
    public async Task GetAllAsync_CallsDatabaseService()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Test1" },
            new() { Id = 2, Title = "Test2" }
        };
        _mockDatabase.Setup(d => d.GetAllCredentialsAsync())
            .ReturnsAsync(credentials);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        _mockDatabase.Verify(d => d.GetAllCredentialsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCredential()
    {
        var credential = new Credential { Id = 1, Title = "Test" };
        _mockDatabase.Setup(d => d.GetCredentialByIdAsync(1))
            .ReturnsAsync(credential);

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        _mockDatabase.Setup(d => d.GetCredentialByIdAsync(999))
            .ReturnsAsync((Credential?)null);

        var result = await _sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_WhenEncryptionNotInitialized_ThrowsException()
    {
        _mockEncryption.Setup(e => e.IsInitialized).Returns(false);
        var credential = new Credential { Title = "Test" };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SaveAsync(credential, "password123"));

        Assert.Contains("locked", exception.Message.ToLower());
    }

    [Fact]
    public async Task SaveAsync_WhenInitialized_EncryptsAndSaves()
    {
        _mockEncryption.Setup(e => e.IsInitialized).Returns(true);
        _mockEncryption.Setup(e => e.Encrypt("plainPassword"))
            .Returns("encryptedPassword");
        _mockDatabase.Setup(d => d.SaveCredentialAsync(It.IsAny<Credential>()))
            .ReturnsAsync(1);

        var credential = new Credential { Title = "Test", Username = "user" };
        var result = await _sut.SaveAsync(credential, "plainPassword");

        Assert.Equal(1, result);
        Assert.Equal("encryptedPassword", credential.EncryptedPassword);
        _mockEncryption.Verify(e => e.Encrypt("plainPassword"), Times.Once);
        _mockDatabase.Verify(d => d.SaveCredentialAsync(credential), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDatabaseDelete()
    {
        _mockDatabase.Setup(d => d.DeleteCredentialAsync(5))
            .ReturnsAsync(1);

        var result = await _sut.DeleteAsync(5);

        Assert.Equal(1, result);
        _mockDatabase.Verify(d => d.DeleteCredentialAsync(5), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ReturnsAllCredentials()
    {
        var credentials = new List<Credential> { new() { Id = 1, Title = "Test" } };
        _mockDatabase.Setup(d => d.GetAllCredentialsAsync())
            .ReturnsAsync(credentials);

        var result = await _sut.SearchAsync("");

        Assert.Single(result);
        _mockDatabase.Verify(d => d.GetAllCredentialsAsync(), Times.Once);
        _mockDatabase.Verify(d => d.SearchCredentialsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WithWhitespaceQuery_ReturnsAllCredentials()
    {
        var credentials = new List<Credential> { new() { Id = 1, Title = "Test" } };
        _mockDatabase.Setup(d => d.GetAllCredentialsAsync())
            .ReturnsAsync(credentials);

        var result = await _sut.SearchAsync("   ");

        Assert.Single(result);
        _mockDatabase.Verify(d => d.GetAllCredentialsAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_CallsSearchOnDatabase()
    {
        var credentials = new List<Credential> { new() { Id = 1, Title = "Google" } };
        _mockDatabase.Setup(d => d.SearchCredentialsAsync("google"))
            .ReturnsAsync(credentials);

        var result = await _sut.SearchAsync("google");

        Assert.Single(result);
        _mockDatabase.Verify(d => d.SearchCredentialsAsync("google"), Times.Once);
    }

    [Fact]
    public void DecryptPassword_WhenNotInitialized_ThrowsException()
    {
        _mockEncryption.Setup(e => e.IsInitialized).Returns(false);
        var credential = new Credential { EncryptedPassword = "encrypted" };

        var exception = Assert.Throws<InvalidOperationException>(
            () => _sut.DecryptPassword(credential));

        Assert.Contains("locked", exception.Message.ToLower());
    }

    [Fact]
    public void DecryptPassword_WhenInitialized_DecryptsPassword()
    {
        _mockEncryption.Setup(e => e.IsInitialized).Returns(true);
        _mockEncryption.Setup(e => e.Decrypt("encryptedPassword"))
            .Returns("decryptedPassword");

        var credential = new Credential { EncryptedPassword = "encryptedPassword" };
        var result = _sut.DecryptPassword(credential);

        Assert.Equal("decryptedPassword", result);
        _mockEncryption.Verify(e => e.Decrypt("encryptedPassword"), Times.Once);
    }
}
