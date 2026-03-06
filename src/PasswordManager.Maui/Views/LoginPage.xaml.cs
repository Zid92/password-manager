using PasswordManager.Maui.ViewModels;

namespace PasswordManager.Maui.Views;

public partial class LoginPage : ContentPage
{
    private LoginViewModel? _viewModel;

    public LoginPage()
    {
        InitializeComponent();
    }

    public LoginPage(LoginViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_viewModel == null && Handler?.MauiContext?.Services != null)
        {
            _viewModel = Handler.MauiContext.Services.GetRequiredService<LoginViewModel>();
            BindingContext = _viewModel;
        }
        
        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();
        }
    }
}
