using PasswordManager.Core.Models;

namespace PasswordManager.Tests.Models;

public class CredentialTests
{
    [Fact]
    public void Credential_DefaultValues()
    {
        var credential = new Credential();

        Assert.Equal(0, credential.Id);
        Assert.Equal(string.Empty, credential.Title);
        Assert.Equal(string.Empty, credential.Username);
        Assert.Equal(string.Empty, credential.EncryptedPassword);
        Assert.Null(credential.Url);
        Assert.Null(credential.Notes);
        Assert.Null(credential.Category);
        Assert.False(credential.IsBreached);
        Assert.Null(credential.LastBreachCheck);
    }

    [Fact]
    public void Credential_CreatedAtIsSetToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var credential = new Credential();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(credential.CreatedAt >= before);
        Assert.True(credential.CreatedAt <= after);
    }

    [Fact]
    public void Credential_UpdatedAtIsSetToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var credential = new Credential();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(credential.UpdatedAt >= before);
        Assert.True(credential.UpdatedAt <= after);
    }

    [Fact]
    public void Credential_CanSetAllProperties()
    {
        var now = DateTime.UtcNow;
        var credential = new Credential
        {
            Id = 42,
            Title = "My Account",
            Username = "user@example.com",
            EncryptedPassword = "encryptedBase64==",
            Url = "https://example.com",
            Notes = "Some notes here",
            Category = "Work",
            IsBreached = true,
            LastBreachCheck = now,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now
        };

        Assert.Equal(42, credential.Id);
        Assert.Equal("My Account", credential.Title);
        Assert.Equal("user@example.com", credential.Username);
        Assert.Equal("encryptedBase64==", credential.EncryptedPassword);
        Assert.Equal("https://example.com", credential.Url);
        Assert.Equal("Some notes here", credential.Notes);
        Assert.Equal("Work", credential.Category);
        Assert.True(credential.IsBreached);
        Assert.Equal(now, credential.LastBreachCheck);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("This is a very long title that should still work fine")]
    public void Credential_Title_AcceptsVariousLengths(string title)
    {
        var credential = new Credential { Title = title };

        Assert.Equal(title, credential.Title);
    }

    [Theory]
    [InlineData("user")]
    [InlineData("user@domain.com")]
    [InlineData("user+tag@domain.co.uk")]
    public void Credential_Username_AcceptsVariousFormats(string username)
    {
        var credential = new Credential { Username = username };

        Assert.Equal(username, credential.Username);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://sub.domain.example.co.uk/path?query=1")]
    public void Credential_Url_AcceptsValidUrls(string url)
    {
        var credential = new Credential { Url = url };

        Assert.Equal(url, credential.Url);
    }
}
