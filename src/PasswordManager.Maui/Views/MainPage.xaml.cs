using PasswordManager.Maui.ViewModels;

namespace PasswordManager.Maui.Views;

public partial class MainPage : ContentPage
{
    private MainViewModel? _viewModel;

    public MainPage()
    {
        InitializeComponent();
    }

    public MainPage(MainViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_viewModel == null && Handler?.MauiContext?.Services != null)
        {
            _viewModel = Handler.MauiContext.Services.GetRequiredService<MainViewModel>();
            BindingContext = _viewModel;
        }
        
        if (_viewModel != null)
        {
            await _viewModel.LoadCredentialsAsync();
        }
    }
}
