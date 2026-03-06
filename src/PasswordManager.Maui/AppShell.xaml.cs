using PasswordManager.Maui.Views;

namespace PasswordManager.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}
