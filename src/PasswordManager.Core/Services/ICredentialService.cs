using PasswordManager.Core.Models;

namespace PasswordManager.Core.Services;

public interface ICredentialService
{
    Task<List<Credential>> GetAllAsync();
    Task<Credential?> GetByIdAsync(int id);
    Task<int> SaveAsync(Credential credential, string plainPassword);
    Task<int> DeleteAsync(int id);
    Task<List<Credential>> SearchAsync(string query);
    string DecryptPassword(Credential credential);
}
