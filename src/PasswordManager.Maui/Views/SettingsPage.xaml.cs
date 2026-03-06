using PasswordManager.Maui.ViewModels;

namespace PasswordManager.Maui.Views;

public partial class SettingsPage : ContentPage
{
    private SettingsViewModel? _viewModel;

    public SettingsPage()
    {
        InitializeComponent();
    }

    public SettingsPage(SettingsViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_viewModel == null && Handler?.MauiContext?.Services != null)
        {
            _viewModel = Handler.MauiContext.Services.GetRequiredService<SettingsViewModel>();
            BindingContext = _viewModel;
        }
        
        if (_viewModel != null)
        {
            await _viewModel.LoadSettingsAsync();
        }
    }
}
