using PasswordManager.Core.Data;
using PasswordManager.Core.Models;
using SQLite;

namespace PasswordManager.Maui.Services;

public class MauiDatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public MauiDatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "vault.db");
    }

    public async Task InitializeAsync(string password)
    {
        var options = new SQLiteConnectionString(
            _dbPath,
            true,
            key: password);

        _database = new SQLiteAsyncConnection(options);

        await _database.CreateTableAsync<Credential>();
        await _database.CreateTableAsync<UsageHistory>();
        await _database.CreateTableAsync<AppSettings>();
    }

    public Task<bool> IsFirstRunAsync()
    {
        return Task.FromResult(!File.Exists(_dbPath));
    }

    public async Task<List<Credential>> GetAllCredentialsAsync()
    {
        EnsureInitialized();
        return await _database!.Table<Credential>()
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Credential?> GetCredentialByIdAsync(int id)
    {
        EnsureInitialized();
        return await _database!.Table<Credential>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> SaveCredentialAsync(Credential credential)
    {
        EnsureInitialized();
        credential.UpdatedAt = DateTime.UtcNow;

        if (credential.Id == 0)
        {
            credential.CreatedAt = DateTime.UtcNow;
            return await _database!.InsertAsync(credential);
        }

        return await _database!.UpdateAsync(credential);
    }

    public async Task<int> DeleteCredentialAsync(int id)
    {
        EnsureInitialized();
        return await _database!.DeleteAsync<Credential>(id);
    }

    public async Task<List<Credential>> SearchCredentialsAsync(string query)
    {
        EnsureInitialized();
        var lowerQuery = query.ToLowerInvariant();

        return await _database!.Table<Credential>()
            .Where(c => c.Title.ToLower().Contains(lowerQuery)
                     || c.Username.ToLower().Contains(lowerQuery)
                     || (c.Url != null && c.Url.ToLower().Contains(lowerQuery)))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        EnsureInitialized();
        var setting = await _database!.Table<AppSettings>()
            .FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task SetSettingAsync(string key, string value)
    {
        EnsureInitialized();
        var existing = await _database!.Table<AppSettings>()
            .FirstOrDefaultAsync(s => s.Key == key);

        if (existing != null)
        {
            existing.Value = value;
            await _database.UpdateAsync(existing);
        }
        else
        {
            await _database.InsertAsync(new AppSettings { Key = key, Value = value });
        }
    }

    public async Task AddUsageHistoryAsync(UsageHistory history)
    {
        EnsureInitialized();
        await _database!.InsertAsync(history);
    }

    public async Task<List<UsageHistory>> GetUsageHistoryForContextAsync(string processName, string? windowTitle)
    {
        EnsureInitialized();

        var query = _database!.Table<UsageHistory>()
            .Where(h => h.ProcessName == processName);

        return await query
            .OrderByDescending(h => h.UsedAt)
            .Take(50)
            .ToListAsync();
    }

    public void Close()
    {
        _database?.CloseAsync().Wait();
        _database = null;
    }

    private void EnsureInitialized()
    {
        if (_database == null)
            throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");
    }
}
