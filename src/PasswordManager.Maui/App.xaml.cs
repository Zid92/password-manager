namespace PasswordManager.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"[FATAL] Unhandled exception: {ex}");
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[FATAL] Unobserved task exception: {e.Exception}");
            e.SetObserved();
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}