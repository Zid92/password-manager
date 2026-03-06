using PasswordManager.Models;

namespace PasswordManager.Tests.Models;

public class UsageHistoryTests
{
    [Fact]
    public void UsageHistory_DefaultValues()
    {
        var history = new UsageHistory();

        Assert.Equal(0, history.Id);
        Assert.Equal(0, history.CredentialId);
        Assert.Equal(string.Empty, history.ProcessName);
        Assert.Null(history.WindowTitle);
        Assert.Equal(UsageType.CopyUsername, history.Type);
    }

    [Fact]
    public void UsageHistory_UsedAtIsSetToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var history = new UsageHistory();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(history.UsedAt >= before);
        Assert.True(history.UsedAt <= after);
    }

    [Fact]
    public void UsageHistory_CanSetAllProperties()
    {
        var now = DateTime.UtcNow;
        var history = new UsageHistory
        {
            Id = 100,
            CredentialId = 5,
            ProcessName = "chrome.exe",
            WindowTitle = "Google - Search",
            UsedAt = now,
            Type = UsageType.AutoFill
        };

        Assert.Equal(100, history.Id);
        Assert.Equal(5, history.CredentialId);
        Assert.Equal("chrome.exe", history.ProcessName);
        Assert.Equal("Google - Search", history.WindowTitle);
        Assert.Equal(now, history.UsedAt);
        Assert.Equal(UsageType.AutoFill, history.Type);
    }

    [Theory]
    [InlineData("notepad.exe")]
    [InlineData("chrome.exe")]
    [InlineData("Microsoft.WindowsTerminal.exe")]
    [InlineData("SOME_APP.EXE")]
    public void UsageHistory_ProcessName_AcceptsVariousFormats(string processName)
    {
        var history = new UsageHistory { ProcessName = processName };

        Assert.Equal(processName, history.ProcessName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Simple Title")]
    [InlineData("Very Long Window Title - Application Name - Some Additional Info")]
    public void UsageHistory_WindowTitle_AcceptsNullAndStrings(string? windowTitle)
    {
        var history = new UsageHistory { WindowTitle = windowTitle };

        Assert.Equal(windowTitle, history.WindowTitle);
    }
}

public class UsageTypeTests
{
    [Fact]
    public void UsageType_HasExpectedValues()
    {
        Assert.Equal(0, (int)UsageType.CopyUsername);
        Assert.Equal(1, (int)UsageType.CopyPassword);
        Assert.Equal(2, (int)UsageType.AutoFill);
    }

    [Fact]
    public void UsageType_HasThreeValues()
    {
        var values = Enum.GetValues<UsageType>();

        Assert.Equal(3, values.Length);
    }

    [Theory]
    [InlineData(UsageType.CopyUsername, "CopyUsername")]
    [InlineData(UsageType.CopyPassword, "CopyPassword")]
    [InlineData(UsageType.AutoFill, "AutoFill")]
    public void UsageType_ToStringReturnsExpectedName(UsageType type, string expected)
    {
        Assert.Equal(expected, type.ToString());
    }
}
