namespace PasswordManager.Core.Services;

/// <summary>
/// Indicates whether the app is using the central HTTP API (remote vault) or local storage.
/// When true, unlock flow skips local hash/salt verification and relies on the server.
/// </summary>
public interface IUseRemoteApi
{
    bool UseRemoteApi { get; }
}
