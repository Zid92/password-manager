using PasswordManager.Models;
using PasswordManager.ViewModels;

namespace PasswordManager.Tests.ViewModels;

public class CredentialItemViewModelTests
{
    [Fact]
    public void FromModel_MapsAllProperties()
    {
        var credential = new Credential
        {
            Id = 42,
            Title = "Test Account",
            Username = "testuser",
            Url = "https://test.com",
            IsBreached = true,
            UpdatedAt = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc)
        };

        var viewModel = CredentialItemViewModel.FromModel(credential);

        Assert.Equal(42, viewModel.Id);
        Assert.Equal("Test Account", viewModel.Title);
        Assert.Equal("testuser", viewModel.Username);
        Assert.Equal("https://test.com", viewModel.Url);
        Assert.True(viewModel.IsBreached);
        Assert.Equal(new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc), viewModel.UpdatedAt);
    }

    [Fact]
    public void FromModel_WithNullUrl_SetsNullUrl()
    {
        var credential = new Credential
        {
            Id = 1,
            Title = "Test",
            Username = "user",
            Url = null
        };

        var viewModel = CredentialItemViewModel.FromModel(credential);

        Assert.Null(viewModel.Url);
    }

    [Fact]
    public void FromModel_WithNotBreached_SetsFalse()
    {
        var credential = new Credential
        {
            Id = 1,
            Title = "Test",
            Username = "user",
            IsBreached = false
        };

        var viewModel = CredentialItemViewModel.FromModel(credential);

        Assert.False(viewModel.IsBreached);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var viewModel = new CredentialItemViewModel();

        Assert.Equal(0, viewModel.Id);
        Assert.Equal(string.Empty, viewModel.Title);
        Assert.Equal(string.Empty, viewModel.Username);
        Assert.Null(viewModel.Url);
        Assert.False(viewModel.IsBreached);
        Assert.Equal(default(DateTime), viewModel.UpdatedAt);
    }

    [Fact]
    public void Properties_AreSettable()
    {
        var viewModel = new CredentialItemViewModel
        {
            Id = 10,
            Title = "Updated Title",
            Username = "newuser",
            Url = "https://new.com",
            IsBreached = true,
            UpdatedAt = DateTime.UtcNow
        };

        Assert.Equal(10, viewModel.Id);
        Assert.Equal("Updated Title", viewModel.Title);
        Assert.Equal("newuser", viewModel.Username);
        Assert.Equal("https://new.com", viewModel.Url);
        Assert.True(viewModel.IsBreached);
    }

    [Fact]
    public void PropertyChanged_IsRaisedWhenTitleChanges()
    {
        var viewModel = new CredentialItemViewModel();
        var propertyChangedRaised = false;
        var changedPropertyName = string.Empty;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        viewModel.Title = "New Title";

        Assert.True(propertyChangedRaised);
        Assert.Equal("Title", changedPropertyName);
    }

    [Fact]
    public void PropertyChanged_IsRaisedWhenIsBreachedChanges()
    {
        var viewModel = new CredentialItemViewModel();
        var changedProperties = new List<string?>();

        viewModel.PropertyChanged += (sender, args) =>
        {
            changedProperties.Add(args.PropertyName);
        };

        viewModel.IsBreached = true;

        Assert.Contains("IsBreached", changedProperties);
    }

    [Fact]
    public void FromModel_WithEmptyStrings_MapsCorrectly()
    {
        var credential = new Credential
        {
            Id = 1,
            Title = "",
            Username = ""
        };

        var viewModel = CredentialItemViewModel.FromModel(credential);

        Assert.Equal("", viewModel.Title);
        Assert.Equal("", viewModel.Username);
    }

    [Fact]
    public void FromModel_DoesNotMapEncryptedPassword()
    {
        var credential = new Credential
        {
            Id = 1,
            Title = "Test",
            Username = "user",
            EncryptedPassword = "supersecret=="
        };

        var viewModel = CredentialItemViewModel.FromModel(credential);

        var type = viewModel.GetType();
        var passwordProperty = type.GetProperty("Password");
        var encryptedPasswordProperty = type.GetProperty("EncryptedPassword");

        Assert.Null(passwordProperty);
        Assert.Null(encryptedPasswordProperty);
    }
}
