using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PasswordManager.ApiClient;
using PasswordManager.Core.Data;
using PasswordManager.Core.Services;
using PasswordManager.Maui.Services;
using PasswordManager.Maui.ViewModels;
using PasswordManager.Maui.Views;

namespace PasswordManager.Maui;

public static class MauiProgram
{
    private const string ApiBaseUrl = "http://localhost:5131";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // All clients use the central API; no local vault.
        builder.Services.AddSingleton<IUseRemoteApi>(new UseRemoteApiFlag(useRemoteApi: true));
        builder.Services.AddSingleton<HttpClient>(_ => new HttpClient { BaseAddress = new Uri(ApiBaseUrl) });
        builder.Services.AddSingleton<IDatabaseService, ApiBackedDatabaseService>();
        builder.Services.AddSingleton<IEncryptionService, RemoteStubEncryptionService>();
        builder.Services.AddSingleton<ICredentialService, ApiBackedCredentialService>();

        builder.Services.AddSingleton<IBreachCheckService, BreachCheckService>();
        builder.Services.AddSingleton<IRankingService, RankingService>();
        builder.Services.AddSingleton<IBiometricService, MauiBiometricService>();
        builder.Services.AddSingleton<ISecureStorageService, MauiSecureStorageService>();
        builder.Services.AddSingleton<IMauiNavigationService, MauiNavigationService>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Initialize ServiceLocator for pages using Shell DataTemplate (parameterless constructor)
        ServiceLocator.Initialize(app.Services);

        return app;
    }
}
