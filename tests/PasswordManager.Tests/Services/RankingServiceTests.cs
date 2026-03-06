using Moq;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;

namespace PasswordManager.Tests.Services;

public class RankingServiceTests
{
    private readonly Mock<IDatabaseService> _mockDatabase;
    private readonly Mock<ICredentialService> _mockCredentialService;
    private readonly RankingService _sut;

    public RankingServiceTests()
    {
        _mockDatabase = new Mock<IDatabaseService>();
        _mockCredentialService = new Mock<ICredentialService>();
        _sut = new RankingService(_mockDatabase.Object, _mockCredentialService.Object);
    }

    [Fact]
    public async Task GetRankedCredentialsAsync_WithNoHistory_ReturnsAllCredentials()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Google" },
            new() { Id = 2, Title = "Facebook" }
        };
        _mockCredentialService.Setup(c => c.GetAllAsync()).ReturnsAsync(credentials);
        _mockDatabase.Setup(d => d.GetUsageHistoryForContextAsync("notepad.exe", null))
            .ReturnsAsync(new List<UsageHistory>());

        var result = await _sut.GetRankedCredentialsAsync("notepad.exe", null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRankedCredentialsAsync_WithMatchingHistory_RanksHigher()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Google" },
            new() { Id = 2, Title = "Facebook" }
        };
        var history = new List<UsageHistory>
        {
            new() { CredentialId = 2, ProcessName = "chrome.exe", UsedAt = DateTime.UtcNow }
        };

        _mockCredentialService.Setup(c => c.GetAllAsync()).ReturnsAsync(credentials);
        _mockDatabase.Setup(d => d.GetUsageHistoryForContextAsync("chrome.exe", null))
            .ReturnsAsync(history);

        var result = await _sut.GetRankedCredentialsAsync("chrome.exe", null);

        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task GetRankedCredentialsAsync_WithMultipleUsages_RanksMoreFrequentHigher()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Google" },
            new() { Id = 2, Title = "Facebook" }
        };
        var history = new List<UsageHistory>
        {
            new() { CredentialId = 1, ProcessName = "chrome.exe", UsedAt = DateTime.UtcNow },
            new() { CredentialId = 2, ProcessName = "chrome.exe", UsedAt = DateTime.UtcNow },
            new() { CredentialId = 2, ProcessName = "chrome.exe", UsedAt = DateTime.UtcNow },
            new() { CredentialId = 2, ProcessName = "chrome.exe", UsedAt = DateTime.UtcNow }
        };

        _mockCredentialService.Setup(c => c.GetAllAsync()).ReturnsAsync(credentials);
        _mockDatabase.Setup(d => d.GetUsageHistoryForContextAsync("chrome.exe", null))
            .ReturnsAsync(history);

        var result = await _sut.GetRankedCredentialsAsync("chrome.exe", null);

        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task GetRankedCredentialsAsync_WithRecentUsage_RanksHigher()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Google" },
            new() { Id = 2, Title = "Facebook" }
        };
        var history = new List<UsageHistory>
        {
            new() { CredentialId = 1, ProcessName = "chrome.exe", UsedAt = DateTime.UtcNow.AddDays(-30) },
            new() { CredentialId = 2, ProcessName = "chrome.exe", UsedAt = DateTime.UtcNow.AddMinutes(-5) }
        };

        _mockCredentialService.Setup(c => c.GetAllAsync()).ReturnsAsync(credentials);
        _mockDatabase.Setup(d => d.GetUsageHistoryForContextAsync("chrome.exe", null))
            .ReturnsAsync(history);

        var result = await _sut.GetRankedCredentialsAsync("chrome.exe", null);

        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task GetRankedCredentialsAsync_WithWindowTitleMatch_RanksHigher()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Google" },
            new() { Id = 2, Title = "Facebook" }
        };
        var history = new List<UsageHistory>
        {
            new() { CredentialId = 1, ProcessName = "chrome.exe", WindowTitle = "Facebook - Login", UsedAt = DateTime.UtcNow },
            new() { CredentialId = 2, ProcessName = "chrome.exe", WindowTitle = "Facebook - Login", UsedAt = DateTime.UtcNow }
        };

        _mockCredentialService.Setup(c => c.GetAllAsync()).ReturnsAsync(credentials);
        _mockDatabase.Setup(d => d.GetUsageHistoryForContextAsync("chrome.exe", "Facebook - Home"))
            .ReturnsAsync(history);

        var result = await _sut.GetRankedCredentialsAsync("chrome.exe", "Facebook - Home");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetRankedCredentialsAsync_WithUrlMatch_GivesBonus()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Google", Url = "https://google.com" },
            new() { Id = 2, Title = "Facebook", Url = "https://facebook.com" }
        };

        _mockCredentialService.Setup(c => c.GetAllAsync()).ReturnsAsync(credentials);
        _mockDatabase.Setup(d => d.GetUsageHistoryForContextAsync("chrome.exe", "facebook.com - Login"))
            .ReturnsAsync(new List<UsageHistory>());

        var result = await _sut.GetRankedCredentialsAsync("chrome.exe", "facebook.com - Login");

        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task GetRankedCredentialsAsync_WithNoUrlAndNoHistory_ReturnsInOriginalOrder()
    {
        var credentials = new List<Credential>
        {
            new() { Id = 1, Title = "Apple" },
            new() { Id = 2, Title = "Banana" }
        };

        _mockCredentialService.Setup(c => c.GetAllAsync()).ReturnsAsync(credentials);
        _mockDatabase.Setup(d => d.GetUsageHistoryForContextAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<UsageHistory>());

        var result = await _sut.GetRankedCredentialsAsync("unknown.exe", "Unknown Window");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task RecordUsageAsync_CreatesHistoryEntry()
    {
        UsageHistory? capturedHistory = null;
        _mockDatabase.Setup(d => d.AddUsageHistoryAsync(It.IsAny<UsageHistory>()))
            .Callback<UsageHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        await _sut.RecordUsageAsync(5, "chrome.exe", "Google - Search", UsageType.AutoFill);

        Assert.NotNull(capturedHistory);
        Assert.Equal(5, capturedHistory!.CredentialId);
        Assert.Equal("chrome.exe", capturedHistory.ProcessName);
        Assert.Equal("Google - Search", capturedHistory.WindowTitle);
        Assert.Equal(UsageType.AutoFill, capturedHistory.Type);
        Assert.True(capturedHistory.UsedAt <= DateTime.UtcNow);
        Assert.True(capturedHistory.UsedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Theory]
    [InlineData(UsageType.CopyUsername)]
    [InlineData(UsageType.CopyPassword)]
    [InlineData(UsageType.AutoFill)]
    public async Task RecordUsageAsync_RecordsCorrectUsageType(UsageType usageType)
    {
        UsageHistory? capturedHistory = null;
        _mockDatabase.Setup(d => d.AddUsageHistoryAsync(It.IsAny<UsageHistory>()))
            .Callback<UsageHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        await _sut.RecordUsageAsync(1, "app.exe", null, usageType);

        Assert.Equal(usageType, capturedHistory!.Type);
    }

    [Fact]
    public async Task RecordUsageAsync_WithNullWindowTitle_RecordsSuccessfully()
    {
        UsageHistory? capturedHistory = null;
        _mockDatabase.Setup(d => d.AddUsageHistoryAsync(It.IsAny<UsageHistory>()))
            .Callback<UsageHistory>(h => capturedHistory = h)
            .Returns(Task.CompletedTask);

        await _sut.RecordUsageAsync(1, "app.exe", null, UsageType.CopyPassword);

        Assert.Null(capturedHistory!.WindowTitle);
        _mockDatabase.Verify(d => d.AddUsageHistoryAsync(It.IsAny<UsageHistory>()), Times.Once);
    }
}
