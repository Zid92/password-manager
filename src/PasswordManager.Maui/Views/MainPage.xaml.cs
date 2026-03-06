using PasswordManager.Maui.ViewModels;

namespace PasswordManager.Maui.Views;

public partial class MainPage : ContentPage
{
    private MainViewModel? _viewModel;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel == null)
        {
            _viewModel = ServiceLocator.GetService<MainViewModel>();
            BindingContext = _viewModel;
        }

        await _viewModel.LoadCredentialsAsync();
    }
}
