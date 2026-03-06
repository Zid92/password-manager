namespace PasswordManager.Maui;

public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("ServiceLocator not initialized.");

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static T GetService<T>() where T : notnull =>
        Services.GetRequiredService<T>();
}
