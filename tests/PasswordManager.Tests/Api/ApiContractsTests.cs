using PasswordManager.Contracts;
using Xunit;

namespace PasswordManager.Tests.Api;

public class ApiContractsTests
{
    [Fact]
    public void SaveCredentialRequest_HasExpectedDefaults()
    {
        var request = new SaveCredentialRequest();

        Assert.Equal(string.Empty, request.Title);
        Assert.Equal(string.Empty, request.Username);
        Assert.Equal(string.Empty, request.PlainPassword);
    }

    [Fact]
    public void OpenVaultRequest_StoresMasterPassword()
    {
        var request = new OpenVaultRequest("secret");
        Assert.Equal("secret", request.MasterPassword);
    }
}

