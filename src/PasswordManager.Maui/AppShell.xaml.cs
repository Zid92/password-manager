using PasswordManager.Maui.Views;

namespace PasswordManager.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // "login" and "main" routes are declared in AppShell.xaml via ShellContent Route attribute
        // Register only "settings" which is accessed via Shell.GoToAsync("settings")
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}
