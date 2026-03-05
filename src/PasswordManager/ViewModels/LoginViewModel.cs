using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Data;
using PasswordManager.Models;
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

    [RelayCommand]
    private async Task UnlockAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(MasterPassword))
            {
                ErrorMessage = "Введите мастер-пароль";
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
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateVaultAsync()
    {
        if (MasterPassword != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return;
        }

        if (MasterPassword.Length < 8)
        {
            ErrorMessage = "Пароль должен быть минимум 8 символов";
            return;
        }

        // Initialize database with master password
        await _databaseService.InitializeAsync(MasterPassword);

        // Generate salt and hash master password for verification
        var salt = _encryptionService.GenerateSalt();
        var hash = _encryptionService.HashPassword(MasterPassword, salt);

        // Store hash and salt in settings
        await _databaseService.SetSettingAsync(SettingsKeys.MasterPasswordHash, hash);
        await _databaseService.SetSettingAsync(SettingsKeys.Salt, Convert.ToBase64String(salt));

        // Initialize encryption service
        _encryptionService.Initialize(MasterPassword);

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
                ErrorMessage = "Хранилище повреждено";
                return;
            }

            var salt = Convert.FromBase64String(storedSaltBase64);
            
            if (!_encryptionService.VerifyPassword(MasterPassword, storedHash, salt))
            {
                _databaseService.Close();
                ErrorMessage = "Неверный мастер-пароль";
                return;
            }

            // Initialize encryption service
            _encryptionService.Initialize(MasterPassword);

            _navigationService.NavigateToMain();
        }
        catch (SQLite.SQLiteException)
        {
            ErrorMessage = "Неверный мастер-пароль";
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
                ErrorMessage = "Windows Hello недоступен на этом устройстве";
                return;
            }

            // Authenticate with Windows Hello
            var authenticated = await _windowsHelloService.AuthenticateAsync(
                "Разблокировать Password Manager");

            if (!authenticated)
            {
                ErrorMessage = "Аутентификация отменена";
                return;
            }

            // Get stored password
            var storedPassword = await _windowsHelloService.GetStoredPasswordAsync();
            if (storedPassword == null)
            {
                ErrorMessage = "Windows Hello не настроен. Войдите с мастер-паролем";
                return;
            }

            MasterPassword = storedPassword;
            await UnlockVaultAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка Windows Hello: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
