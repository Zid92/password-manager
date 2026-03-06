using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.Maui.Services;

namespace PasswordManager.Maui.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ICredentialService _credentialService;
    private readonly IEncryptionService _encryptionService;
    private readonly IBreachCheckService _breachCheckService;
    private readonly IMauiNavigationService _navigationService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CredentialItemViewModel> _credentials = new();

    [ObservableProperty]
    private CredentialItemViewModel? _selectedCredential;

    [ObservableProperty]
    private bool _hasCredentials;

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

    [ObservableProperty]
    private bool _isBreachCheckRunning;

    [ObservableProperty]
    private string _breachCheckStatus = string.Empty;

    [ObservableProperty]
    private int _breachCheckProgress;

    [ObservableProperty]
    private bool _isDialogPasswordHidden = true;

    private int _editingCredentialId;

    public MainViewModel(
        ICredentialService credentialService,
        IEncryptionService encryptionService,
        IBreachCheckService breachCheckService,
        IMauiNavigationService navigationService)
    {
        _credentialService = credentialService;
        _encryptionService = encryptionService;
        _breachCheckService = breachCheckService;
        _navigationService = navigationService;
    }

    public async Task LoadCredentialsAsync()
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

    partial void OnSearchTextChanged(string value)
    {
        _ = SearchCredentialsAsync(value);
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
    private void ToggleDialogPassword() => IsDialogPasswordHidden = !IsDialogPasswordHidden;

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

        bool confirmed = await Shell.Current.CurrentPage.DisplayAlert(
            "Confirm deletion",
            $"Delete entry \"{item.Title}\"?",
            "Yes", "No");

        if (confirmed)
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
            SetError("Title is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(DialogPassword))
        {
            SetError("Password is required");
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

        await Clipboard.Default.SetTextAsync(item.Username);
        await Shell.Current.CurrentPage.DisplayAlert("Copied", "Username copied to clipboard", "OK");
    }

    [RelayCommand]
    private async Task CopyPasswordAsync(CredentialItemViewModel? item)
    {
        if (item == null) return;

        var credential = await _credentialService.GetByIdAsync(item.Id);
        if (credential == null) return;

        var password = _credentialService.DecryptPassword(credential);
        await Clipboard.Default.SetTextAsync(password);
        await Shell.Current.CurrentPage.DisplayAlert("Copied", "Password copied to clipboard", "OK");
    }

    [RelayCommand]
    private async Task CheckBreachesAsync()
    {
        if (IsBreachCheckRunning) return;

        IsBreachCheckRunning = true;
        BreachCheckProgress = 0;
        BreachCheckStatus = "Starting check...";

        try
        {
            var progress = new Progress<BreachCheckProgress>(p =>
            {
                BreachCheckProgress = (int)((double)p.Current / p.Total * 100);
                BreachCheckStatus = $"Checking: {p.CurrentTitle} ({p.Current}/{p.Total})";
            });

            await _breachCheckService.CheckAllCredentialsAsync(progress);

            await LoadCredentialsAsync();

            var breachedCount = Credentials.Count(c => c.IsBreached);
            if (breachedCount > 0)
            {
                await Shell.Current.CurrentPage.DisplayAlert(
                    "Check completed",
                    $"Found {breachedCount} compromised password(s)! It is recommended to change them.",
                    "OK");
            }
            else
            {
                await Shell.Current.CurrentPage.DisplayAlert(
                    "Check completed",
                    "All passwords are safe!",
                    "OK");
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
    private async Task LogoutAsync()
    {
        _encryptionService.Clear();
        await _navigationService.NavigateToAsync("//login");
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        await _navigationService.NavigateToAsync("settings");
    }

    private void ResetDialog()
    {
        _editingCredentialId = 0;
        DialogTitle = string.Empty;
        DialogUsername = string.Empty;
        DialogPassword = string.Empty;
        DialogUrl = null;
        DialogNotes = null;
        IsDialogPasswordHidden = true;
        ClearError();
    }
}

public partial class CredentialItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string? _url;

    [ObservableProperty]
    private bool _isBreached;

    [ObservableProperty]
    private DateTime _updatedAt;

    public static CredentialItemViewModel FromModel(Credential credential)
    {
        return new CredentialItemViewModel
        {
            Id = credential.Id,
            Title = credential.Title,
            Username = credential.Username,
            Url = credential.Url,
            IsBreached = credential.IsBreached,
            UpdatedAt = credential.UpdatedAt
        };
    }
}
