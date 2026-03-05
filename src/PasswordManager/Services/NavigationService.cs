using System.Windows;
using PasswordManager.Views;
using Application = System.Windows.Application;

namespace PasswordManager.Services;

public class NavigationService : INavigationService
{
    public void NavigateToMain()
    {
        var mainWindow = App.GetService<MainWindow>();
        mainWindow.Show();
        CloseCurrentWindow();
    }

    public void NavigateToLogin()
    {
        var loginWindow = App.GetService<LoginWindow>();
        loginWindow.Show();
        CloseCurrentWindow();
    }

    public void CloseCurrentWindow()
    {
        var currentWindow = Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.IsActive);
        
        currentWindow?.Close();
    }
}
