using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.Maui.Services;

namespace PasswordManager.Maui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IBiometricService _biometricService;
    private readonly IMauiNavigationService _navigationService;

    [ObservableProperty]
    private bool _biometricEnabled;

    [ObservableProperty]
    private bool _isBiometricAvailable;

    public SettingsViewModel(
        IDatabaseService databaseService,
        IBiometricService biometricService,
        IMauiNavigationService navigationService)
    {
        _databaseService = databaseService;
        _biometricService = biometricService;
        _navigationService = navigationService;
    }

    public async Task LoadSettingsAsync()
    {
        IsBiometricAvailable = await _biometricService.IsAvailableAsync();
        BiometricEnabled = await _biometricService.IsEnabledAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (BiometricEnabled && IsBiometricAvailable)
            {
                await _biometricService.SetEnabledAsync(true);
                await _databaseService.SetSettingAsync(SettingsKeys.BiometricEnabled, "true");
            }
            else
            {
                await _biometricService.SetEnabledAsync(false);
                await _databaseService.SetSettingAsync(SettingsKeys.BiometricEnabled, "false");
            }

            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            SetError($"Error saving settings: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.GoBackAsync();
    }
}
