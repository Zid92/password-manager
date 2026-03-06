using PasswordManager.Maui.ViewModels;

namespace PasswordManager.Maui.Views;

public partial class SettingsPage : ContentPage
{
    private SettingsViewModel? _viewModel;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel == null)
        {
            _viewModel = ServiceLocator.GetService<SettingsViewModel>();
            BindingContext = _viewModel;
        }

        await _viewModel.LoadSettingsAsync();
    }
}
