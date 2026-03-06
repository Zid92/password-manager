using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.Services;

namespace PasswordManager.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IDatabaseService _databaseService;
    private readonly IEncryptionService _encryptionService;
    private readonly IWindowsHelloService _windowsHelloService;

    [ObservableProperty]
    private string _masterPassword = string.Empty;

    [ObservableProperty]
    private bool _isFirstRun = true;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _enableWindowsHello;

    [ObservableProperty]
    private bool _isWindowsHelloAvailable;

    [ObservableProperty]
    private bool _hasMasterPasswordError;

    [ObservableProperty]
    private string? _masterPasswordError;

    [ObservableProperty]
    private bool _hasConfirmPasswordError;

    [ObservableProperty]
    private string? _confirmPasswordError;

    public LoginViewModel(
        INavigationService navigationService,
        IDatabaseService databaseService,
        IEncryptionService encryptionService,
        IWindowsHelloService windowsHelloService)
    {
        _navigationService = navigationService;
        _databaseService = databaseService;
        _encryptionService = encryptionService;
        _windowsHelloService = windowsHelloService;

        _ = CheckWindowsHelloAvailabilityAsync();
    }

    private async Task CheckWindowsHelloAvailabilityAsync()
    {
        IsWindowsHelloAvailable = await _windowsHelloService.IsAvailableAsync();
    }

    private void ClearFieldErrors()
    {
        HasMasterPasswordError = false;
        MasterPasswordError = null;
        HasConfirmPasswordError = false;
        ConfirmPasswordError = null;
        ClearError();
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        ClearFieldErrors();
        IsLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(MasterPassword))
            {
                HasMasterPasswordError = true;
                MasterPasswordError = "Master password is required";
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
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateVaultAsync()
    {
        bool hasError = false;

        if (MasterPassword.Length < 8)
        {
            HasMasterPasswordError = true;
            MasterPasswordError = "Password must be at least 8 characters";
            hasError = true;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            HasConfirmPasswordError = true;
            ConfirmPasswordError = "Please confirm your password";
            hasError = true;
        }
        else if (MasterPassword != ConfirmPassword)
        {
            HasConfirmPasswordError = true;
            ConfirmPasswordError = "Passwords do not match";
            hasError = true;
        }

        if (hasError) return;

        // Initialize encryption service FIRST
        _encryptionService.Initialize(MasterPassword);

        // Initialize database with master password
        await _databaseService.InitializeAsync(MasterPassword);

        // Generate salt and hash master password for verification
        var salt = _encryptionService.GenerateSalt();
        var hash = _encryptionService.HashPassword(MasterPassword, salt);

        // Store hash and salt in settings
        await _databaseService.SetSettingAsync(SettingsKeys.MasterPasswordHash, hash);
        await _databaseService.SetSettingAsync(SettingsKeys.Salt, Convert.ToBase64String(salt));

        // Setup Windows Hello if requested
        if (EnableWindowsHello && IsWindowsHelloAvailable)
        {
            var registered = await _windowsHelloService.RegisterAsync();
            if (registered)
            {
                await _windowsHelloService.StorePasswordAsync(MasterPassword);
                await _databaseService.SetSettingAsync(SettingsKeys.WindowsHelloEnabled, "true");
            }
        }

        _navigationService.NavigateToMain();
    }

    private async Task UnlockVaultAsync()
    {
        try
        {
            // Initialize database with master password
            await _databaseService.InitializeAsync(MasterPassword);

            // Verify password
            var storedHash = await _databaseService.GetSettingAsync(SettingsKeys.MasterPasswordHash);
            var storedSaltBase64 = await _databaseService.GetSettingAsync(SettingsKeys.Salt);

            if (storedHash == null || storedSaltBase64 == null)
            {
                ErrorMessage = "Vault is corrupted. Please create a new vault.";
                return;
            }

            var salt = Convert.FromBase64String(storedSaltBase64);
            
            if (!_encryptionService.VerifyPassword(MasterPassword, storedHash, salt))
            {
                _databaseService.Close();
                ErrorMessage = "Incorrect master password. Please try again.";
                return;
            }

            // Initialize encryption service
            _encryptionService.Initialize(MasterPassword);

            _navigationService.NavigateToMain();
        }
        catch (SQLite.SQLiteException)
        {
            ErrorMessage = "Incorrect master password. Please try again.";
        }
    }

    [RelayCommand]
    private async Task UnlockWithWindowsHelloAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            if (!IsWindowsHelloAvailable)
            {
                ErrorMessage = "Windows Hello is not available on this device";
                return;
            }

            // Authenticate with Windows Hello
            var authenticated = await _windowsHelloService.AuthenticateAsync(
                "Unlock Password Manager");

            if (!authenticated)
            {
                ErrorMessage = "Authentication cancelled";
                return;
            }

            // Get stored password
            var storedPassword = await _windowsHelloService.GetStoredPasswordAsync();
            if (storedPassword == null)
            {
                ErrorMessage = "Windows Hello not configured. Please login with master password.";
                return;
            }

            MasterPassword = storedPassword;
            await UnlockVaultAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Windows Hello error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
