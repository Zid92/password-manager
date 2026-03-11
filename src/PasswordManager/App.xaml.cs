using System.Net.Http;
using System.Windows;
using Application = System.Windows.Application;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordManager.ApiClient;
using PasswordManager.Core.Data;
using PasswordManager.Core.Services;
using PasswordManager.Native;
using PasswordManager.Services;
using PasswordManager.ViewModels;
using PasswordManager.Views;

namespace PasswordManager;

public partial class App : Application
{
    private readonly IHost _host;
    private HwndSource? _hwndSource;
    private IGlobalHotkeyService? _hotkeyService;
    private bool _isUnlocked;

    public App()
    {
        // Global exception handlers
        DispatcherUnhandledException += (s, e) =>
        {
            System.Windows.MessageBox.Show($"UI Error: {e.Exception.Message}\n\n{e.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
        
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            System.Windows.MessageBox.Show($"Fatal Error: {ex?.Message}\n\n{ex?.StackTrace}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context, services);
            })
            .Build();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var apiBaseUrl = context.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";

        // All clients use the central API; no local vault.
        services.AddSingleton<IUseRemoteApi>(new UseRemoteApiFlag(useRemoteApi: true));
        services.AddSingleton<HttpClient>(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
        services.AddSingleton<IDatabaseService, ApiBackedDatabaseService>();
        services.AddSingleton<IEncryptionService, RemoteStubEncryptionService>();
        services.AddSingleton<ICredentialService, ApiBackedCredentialService>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IWindowsHelloService, WindowsHelloService>();
        services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
        services.AddSingleton<IActiveWindowService, ActiveWindowService>();
        services.AddSingleton<IRankingService, RankingService>();
        services.AddSingleton<IBreachCheckService, BreachCheckService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<QuickInsertViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<QuickInsertWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var databaseService = _host.Services.GetRequiredService<IDatabaseService>();
        var isFirstRun = await databaseService.IsFirstRunAsync();

        var loginViewModel = _host.Services.GetRequiredService<LoginViewModel>();
        loginViewModel.IsFirstRun = isFirstRun;

        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _hotkeyService?.Unregister();
        _hwndSource?.Dispose();

        var databaseService = _host.Services.GetService<IDatabaseService>();
        databaseService?.Close();

        var encryptionService = _host.Services.GetService<IEncryptionService>();
        encryptionService?.Clear();

        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        var app = (App)Current;
        return app._host.Services.GetRequiredService<T>();
    }

    public static void RegisterGlobalHotkey(Window window)
    {
        var app = (App)Current;
        app.SetupGlobalHotkey(window);
    }

    public static void SetUnlocked(bool unlocked)
    {
        var app = (App)Current;
        app._isUnlocked = unlocked;
    }

    private void SetupGlobalHotkey(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(WndProc);

        _hotkeyService = _host.Services.GetRequiredService<IGlobalHotkeyService>();
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        
        // Default hotkey: Ctrl + Alt + P
        _hotkeyService.Register(hwnd, ModifierKeys.Control | ModifierKeys.Alt, Key.P);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            _hotkeyService?.ProcessHotkeyMessage(wParam);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        if (!_isUnlocked) return;

        var activeWindowService = _host.Services.GetRequiredService<IActiveWindowService>();
        var windowInfo = activeWindowService.GetActiveWindowInfo();

        var quickInsertVm = _host.Services.GetRequiredService<QuickInsertViewModel>();
        quickInsertVm.SetTargetWindow(windowInfo);

        var quickInsertWindow = new QuickInsertWindow(quickInsertVm);
        quickInsertWindow.Show();
    }
}
