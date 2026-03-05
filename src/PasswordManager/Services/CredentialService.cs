using PasswordManager.Data;
using PasswordManager.Models;

namespace PasswordManager.Services;

public class CredentialService : ICredentialService
{
    private readonly IDatabaseService _database;
    private readonly IEncryptionService _encryption;

    public CredentialService(IDatabaseService database, IEncryptionService encryption)
    {
        _database = database;
        _encryption = encryption;
    }

    public async Task<List<Credential>> GetAllAsync()
    {
        return await _database.GetAllCredentialsAsync();
    }

    public async Task<Credential?> GetByIdAsync(int id)
    {
        return await _database.GetCredentialByIdAsync(id);
    }

    public async Task<int> SaveAsync(Credential credential, string plainPassword)
    {
        credential.EncryptedPassword = _encryption.Encrypt(plainPassword);
        return await _database.SaveCredentialAsync(credential);
    }

    public async Task<int> DeleteAsync(int id)
    {
        return await _database.DeleteCredentialAsync(id);
    }

    public async Task<List<Credential>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();
            
        return await _database.SearchCredentialsAsync(query);
    }

    public string DecryptPassword(Credential credential)
    {
        return _encryption.Decrypt(credential.EncryptedPassword);
    }
}
