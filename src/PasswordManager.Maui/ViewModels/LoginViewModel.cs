using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.Maui.Services;

namespace PasswordManager.Maui.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IEncryptionService _encryptionService;
    private readonly IBiometricService _biometricService;
    private readonly IMauiNavigationService _navigationService;

    [ObservableProperty]
    private string _masterPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isFirstRun = true;

    [ObservableProperty]
    private bool _enableBiometric;

    [ObservableProperty]
    private bool _isBiometricAvailable;

    public LoginViewModel(
        IDatabaseService databaseService,
        IEncryptionService encryptionService,
        IBiometricService biometricService,
        IMauiNavigationService navigationService)
    {
        _databaseService = databaseService;
        _encryptionService = encryptionService;
        _biometricService = biometricService;
        _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
        IsFirstRun = await _databaseService.IsFirstRunAsync();
        IsBiometricAvailable = await _biometricService.IsAvailableAsync();
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(MasterPassword))
            {
                SetError("Master password is required");
                return;
            }

            if (IsFirstRun)
            {
                await CreateVaultAsync();
            }
            else
            {
                await UnlockVaultAsync();
            }
        }
        catch (Exception ex)
        {
            SetError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateVaultAsync()
    {
        if (MasterPassword.Length < 8)
        {
            SetError("Password must be at least 8 characters");
            return;
        }

        if (MasterPassword != ConfirmPassword)
        {
            SetError("Passwords do not match");
            return;
        }

        _encryptionService.Initialize(MasterPassword);
        await _databaseService.InitializeAsync(MasterPassword);

        var salt = _encryptionService.GenerateSalt();
        var hash = _encryptionService.HashPassword(MasterPassword, salt);

        await _databaseService.SetSettingAsync(SettingsKeys.MasterPasswordHash, hash);
        await _databaseService.SetSettingAsync(SettingsKeys.Salt, Convert.ToBase64String(salt));

        if (EnableBiometric && IsBiometricAvailable)
        {
            var authenticated = await _biometricService.AuthenticateAsync("Enable biometric unlock");
            if (authenticated)
            {
                await _biometricService.StoreMasterKeyAsync(MasterPassword);
                await _biometricService.SetEnabledAsync(true);
                await _databaseService.SetSettingAsync(SettingsKeys.BiometricEnabled, "true");
            }
        }

        await _navigationService.NavigateToAsync("//main");
    }

    private async Task UnlockVaultAsync()
    {
        try
        {
            await _databaseService.InitializeAsync(MasterPassword);

            var storedHash = await _databaseService.GetSettingAsync(SettingsKeys.MasterPasswordHash);
            var storedSaltBase64 = await _databaseService.GetSettingAsync(SettingsKeys.Salt);

            if (storedHash == null || storedSaltBase64 == null)
            {
                SetError("Vault is corrupted. Please create a new vault.");
                return;
            }

            var salt = Convert.FromBase64String(storedSaltBase64);

            if (!_encryptionService.VerifyPassword(MasterPassword, storedHash, salt))
            {
                _databaseService.Close();
                SetError("Incorrect master password");
                return;
            }

            _encryptionService.Initialize(MasterPassword);
            await _navigationService.NavigateToAsync("//main");
        }
        catch (SQLite.SQLiteException)
        {
            SetError("Incorrect master password");
        }
    }

    [RelayCommand]
    private async Task UnlockWithBiometricAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            if (!IsBiometricAvailable)
            {
                SetError("Biometric authentication is not available");
                return;
            }

            var authenticated = await _biometricService.AuthenticateAsync("Unlock Password Manager");
            if (!authenticated)
            {
                SetError("Authentication cancelled");
                return;
            }

            var storedPassword = await _biometricService.RetrieveMasterKeyAsync();
            if (storedPassword == null)
            {
                SetError("Biometric not configured. Please login with master password.");
                return;
            }

            MasterPassword = storedPassword;
            await UnlockVaultAsync();
        }
        catch (Exception ex)
        {
            SetError($"Biometric error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
