using PasswordManager.Maui.ViewModels;

namespace PasswordManager.Maui.Views;

public partial class LoginPage : ContentPage
{
    private LoginViewModel? _viewModel;

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel == null)
        {
            _viewModel = ServiceLocator.GetService<LoginViewModel>();
            BindingContext = _viewModel;
        }

        await _viewModel.InitializeAsync();
    }
}
