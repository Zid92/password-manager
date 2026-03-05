using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Models;
using PasswordManager.Services;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace PasswordManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ICredentialService _credentialService;
    private readonly IEncryptionService _encryptionService;
    private readonly IBreachCheckService _breachCheckService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CredentialItemViewModel> _credentials = new();

    [ObservableProperty]
    private CredentialItemViewModel? _selectedCredential;

    [ObservableProperty]
    private bool _hasCredentials;

    // Dialog properties
    [ObservableProperty]
    private bool _isDialogOpen;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _dialogTitle = string.Empty;

    [ObservableProperty]
    private string _dialogUsername = string.Empty;

    [ObservableProperty]
    private string _dialogPassword = string.Empty;

    [ObservableProperty]
    private string? _dialogUrl;

    [ObservableProperty]
    private string? _dialogNotes;

    // Breach check properties
    [ObservableProperty]
    private bool _isBreachCheckRunning;

    [ObservableProperty]
    private string _breachCheckStatus = string.Empty;

    [ObservableProperty]
    private int _breachCheckProgress;

    private int _editingCredentialId;

    public MainViewModel(
        INavigationService navigationService,
        ICredentialService credentialService,
        IEncryptionService encryptionService,
        IBreachCheckService breachCheckService)
    {
        _navigationService = navigationService;
        _credentialService = credentialService;
        _encryptionService = encryptionService;
        _breachCheckService = breachCheckService;

        _ = LoadCredentialsAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = SearchCredentialsAsync(value);
    }

    private async Task LoadCredentialsAsync()
    {
        IsLoading = true;
        try
        {
            var credentials = await _credentialService.GetAllAsync();
            UpdateCredentialsList(credentials);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchCredentialsAsync(string query)
    {
        var credentials = await _credentialService.SearchAsync(query);
        UpdateCredentialsList(credentials);
    }

    private void UpdateCredentialsList(List<Credential> credentials)
    {
        Credentials.Clear();
        foreach (var cred in credentials)
        {
            Credentials.Add(CredentialItemViewModel.FromModel(cred));
        }
        HasCredentials = Credentials.Count > 0;
    }

    [RelayCommand]
    private void AddCredential()
    {
        ResetDialog();
        IsEditMode = false;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private async Task EditCredentialAsync(CredentialItemViewModel? item)
    {
        if (item == null) return;

        var credential = await _credentialService.GetByIdAsync(item.Id);
        if (credential == null) return;

        _editingCredentialId = credential.Id;
        DialogTitle = credential.Title;
        DialogUsername = credential.Username;
        DialogPassword = _credentialService.DecryptPassword(credential);
        DialogUrl = credential.Url;
        DialogNotes = credential.Notes;
        
        IsEditMode = true;
        IsDialogOpen = true;
    }

    [RelayCommand]
    private async Task DeleteCredentialAsync(CredentialItemViewModel? item)
    {
        if (item == null) return;

        var result = MessageBox.Show(
            $"Удалить запись \"{item.Title}\"?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await _credentialService.DeleteAsync(item.Id);
            await LoadCredentialsAsync();
        }
    }

    [RelayCommand]
    private async Task SaveCredentialAsync()
    {
        if (string.IsNullOrWhiteSpace(DialogTitle))
        {
            ErrorMessage = "Введите название";
            return;
        }

        if (string.IsNullOrWhiteSpace(DialogPassword))
        {
            ErrorMessage = "Введите пароль";
            return;
        }

        ClearError();

        var credential = new Credential
        {
            Id = IsEditMode ? _editingCredentialId : 0,
            Title = DialogTitle,
            Username = DialogUsername,
            Url = DialogUrl,
            Notes = DialogNotes
        };

        await _credentialService.SaveAsync(credential, DialogPassword);
        
        IsDialogOpen = false;
        ResetDialog();
        await LoadCredentialsAsync();
    }

    [RelayCommand]
    private void CancelDialog()
    {
        IsDialogOpen = false;
        ResetDialog();
    }

    [RelayCommand]
    private async Task CopyUsernameAsync(CredentialItemViewModel? item)
    {
        if (item == null) return;
        
        Clipboard.SetText(item.Username);
    }

    [RelayCommand]
    private async Task CopyPasswordAsync(CredentialItemViewModel? item)
    {
        if (item == null) return;

        var credential = await _credentialService.GetByIdAsync(item.Id);
        if (credential == null) return;

        var password = _credentialService.DecryptPassword(credential);
        Clipboard.SetText(password);
    }

    [RelayCommand]
    private async Task CheckBreachesAsync()
    {
        if (IsBreachCheckRunning) return;

        IsBreachCheckRunning = true;
        BreachCheckProgress = 0;
        BreachCheckStatus = "Начинаем проверку...";

        try
        {
            var progress = new Progress<BreachCheckProgress>(p =>
            {
                BreachCheckProgress = (int)((double)p.Current / p.Total * 100);
                BreachCheckStatus = $"Проверка: {p.CurrentTitle} ({p.Current}/{p.Total})";
            });

            await _breachCheckService.CheckAllCredentialsAsync(progress);
            
            await LoadCredentialsAsync();
            
            var breachedCount = Credentials.Count(c => c.IsBreached);
            if (breachedCount > 0)
            {
                MessageBox.Show(
                    $"Найдено {breachedCount} скомпрометированных паролей! Рекомендуется их сменить.",
                    "Проверка завершена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(
                    "Все пароли безопасны!",
                    "Проверка завершена",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        finally
        {
            IsBreachCheckRunning = false;
            BreachCheckStatus = string.Empty;
            BreachCheckProgress = 0;
        }
    }

    [RelayCommand]
    private void Lock()
    {
        _encryptionService.Clear();
        _navigationService.NavigateToLogin();
    }

    private void ResetDialog()
    {
        _editingCredentialId = 0;
        DialogTitle = string.Empty;
        DialogUsername = string.Empty;
        DialogPassword = string.Empty;
        DialogUrl = null;
        DialogNotes = null;
        ClearError();
    }
}
