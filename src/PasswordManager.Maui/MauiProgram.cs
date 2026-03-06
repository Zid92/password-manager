using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PasswordManager.Core.Data;
using PasswordManager.Core.Services;
using PasswordManager.Maui.Services;
using PasswordManager.Maui.ViewModels;
using PasswordManager.Maui.Views;

namespace PasswordManager.Maui;

public static class MauiProgram
{
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

        // Register services
        builder.Services.AddSingleton<IDatabaseService, MauiDatabaseService>();
        builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
        builder.Services.AddSingleton<ICredentialService, CredentialService>();
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

        return builder.Build();
    }
}
