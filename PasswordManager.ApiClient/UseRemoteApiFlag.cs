using PasswordManager.Core.Services;

namespace PasswordManager.ApiClient;

public sealed class UseRemoteApiFlag : IUseRemoteApi
{
    public UseRemoteApiFlag(bool useRemoteApi)
    {
        UseRemoteApi = useRemoteApi;
    }

    public bool UseRemoteApi { get; }
}
