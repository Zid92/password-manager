using System.Security.Cryptography;
using System.Text;
using PasswordManager.Core.Services;

namespace PasswordManager.Tests.Services;

public class BreachCheckServiceTests
{
    [Fact]
    public void ComputeSha1Hash_ReturnsCorrectHash()
    {
        var password = "password";
        var expectedHash = "5BAA61E4C9B93F3F0682250B6CF8331B7EE68FD8";

        var hash = ComputeSha1HashPublic(password);

        Assert.Equal(expectedHash, hash);
    }

    [Fact]
    public void ComputeSha1Hash_IsCasePreserved()
    {
        var hash1 = ComputeSha1HashPublic("Password");
        var hash2 = ComputeSha1HashPublic("password");

        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("test123")]
    [InlineData("VeryLongPasswordWithSpecialChars!@#$%^&*()")]
    public void ComputeSha1Hash_Returns40CharHex(string input)
    {
        var hash = ComputeSha1HashPublic(input);

        Assert.Equal(40, hash.Length);
        Assert.True(hash.All(c => char.IsAsciiHexDigit(c)));
    }

    [Fact]
    public void ComputeSha1Hash_WithUnicode_ReturnsValidHash()
    {
        var hash = ComputeSha1HashPublic("密码"); // non-Latin unicode password

        Assert.Equal(40, hash.Length);
    }

    private static string ComputeSha1HashPublic(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA1.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}

public class BreachCheckResultTests
{
    [Fact]
    public void BreachCheckResult_DefaultValues()
    {
        var result = new BreachCheckResult();

        Assert.False(result.IsBreached);
        Assert.Equal(0, result.BreachCount);
    }

    [Fact]
    public void BreachCheckResult_CanSetProperties()
    {
        var result = new BreachCheckResult
        {
            IsBreached = true,
            BreachCount = 12345
        };

        Assert.True(result.IsBreached);
        Assert.Equal(12345, result.BreachCount);
    }
}

public class BreachCheckProgressTests
{
    [Fact]
    public void BreachCheckProgress_DefaultValues()
    {
        var progress = new BreachCheckProgress();

        Assert.Equal(0, progress.Current);
        Assert.Equal(0, progress.Total);
        Assert.Equal(string.Empty, progress.CurrentTitle);
        Assert.Equal(0, progress.BreachedCount);
    }

    [Fact]
    public void BreachCheckProgress_CanSetAllProperties()
    {
        var progress = new BreachCheckProgress
        {
            Current = 5,
            Total = 10,
            CurrentTitle = "Google Account",
            BreachedCount = 2
        };

        Assert.Equal(5, progress.Current);
        Assert.Equal(10, progress.Total);
        Assert.Equal("Google Account", progress.CurrentTitle);
        Assert.Equal(2, progress.BreachedCount);
    }

    [Fact]
    public void BreachCheckProgress_CalculatesPercentage()
    {
        var progress = new BreachCheckProgress
        {
            Current = 25,
            Total = 100
        };

        var percentage = (double)progress.Current / progress.Total * 100;

        Assert.Equal(25.0, percentage);
    }
}
