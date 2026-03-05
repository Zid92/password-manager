using System.Windows;
using PasswordManager.Views;
using Application = System.Windows.Application;

namespace PasswordManager.Services;

public class NavigationService : INavigationService
{
    public void NavigateToMain()
    {
        // Find LoginWindow to close it after showing MainWindow
        var loginWindow = Application.Current.Windows
            .OfType<LoginWindow>()
            .FirstOrDefault();

        var mainWindow = App.GetService<MainWindow>();
        mainWindow.Show();
        
        // Close login window after main is shown
        loginWindow?.Close();
    }

    public void NavigateToLogin()
    {
        // Find MainWindow to close it after showing LoginWindow
        var mainWindow = Application.Current.Windows
            .OfType<MainWindow>()
            .FirstOrDefault();

        var loginWindow = App.GetService<LoginWindow>();
        loginWindow.Show();
        
        // Close main window after login is shown
        mainWindow?.Close();
    }

    public void CloseCurrentWindow()
    {
        var currentWindow = Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.IsActive);
        
        currentWindow?.Close();
    }
}
