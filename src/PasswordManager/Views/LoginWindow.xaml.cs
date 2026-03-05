using System.Windows;
using System.Windows.Controls;
using PasswordManager.ViewModels;

namespace PasswordManager.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        MasterPasswordBox.PasswordChanged += MasterPasswordBox_PasswordChanged;
        ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
    }

    private void MasterPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.MasterPassword = ((PasswordBox)sender).Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
    }
}
