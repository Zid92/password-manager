using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using PasswordManager.Native;
using PasswordManager.Services;
using Clipboard = System.Windows.Clipboard;

namespace PasswordManager.ViewModels;

public partial class QuickInsertViewModel : ViewModelBase
{
    private readonly ICredentialService _credentialService;
    private readonly IActiveWindowService _activeWindowService;
    private readonly IRankingService _rankingService;
    private ActiveWindowInfo? _targetWindow;

    public event EventHandler? RequestClose;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CredentialItemViewModel> _credentials = new();

    [ObservableProperty]
    private CredentialItemViewModel? _selectedCredential;

    public QuickInsertViewModel(
        ICredentialService credentialService,
        IActiveWindowService activeWindowService,
        IRankingService rankingService)
    {
        _credentialService = credentialService;
        _activeWindowService = activeWindowService;
        _rankingService = rankingService;
    }

    public void SetTargetWindow(ActiveWindowInfo windowInfo)
    {
        _targetWindow = windowInfo;
    }

    public async Task LoadCredentialsAsync()
    {
        List<Credential> credentials;
        
        if (_targetWindow != null)
        {
            // Use smart ranking based on active window context
            credentials = await _rankingService.GetRankedCredentialsAsync(
                _targetWindow.ProcessName, 
                _targetWindow.WindowTitle);
        }
        else
        {
            credentials = await _credentialService.GetAllAsync();
        }
        
        Credentials.Clear();
        foreach (var cred in credentials)
        {
            Credentials.Add(CredentialItemViewModel.FromModel(cred));
        }

        if (Credentials.Count > 0)
        {
            SelectedCredential = Credentials[0];
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = FilterCredentialsAsync(value);
    }

    private async Task FilterCredentialsAsync(string query)
    {
        var credentials = await _credentialService.SearchAsync(query);
        Credentials.Clear();
        foreach (var cred in credentials)
        {
            Credentials.Add(CredentialItemViewModel.FromModel(cred));
        }

        if (Credentials.Count > 0)
        {
            SelectedCredential = Credentials[0];
        }
    }

    [RelayCommand]
    private async Task InsertUsernameAsync()
    {
        if (SelectedCredential == null) return;
        
        var username = SelectedCredential.Username;
        
        // Record usage
        if (_targetWindow != null)
        {
            await _rankingService.RecordUsageAsync(
                SelectedCredential.Id,
                _targetWindow.ProcessName,
                _targetWindow.WindowTitle,
                UsageType.CopyUsername);
        }
        
        RequestClose?.Invoke(this, EventArgs.Empty);
        await Task.Delay(150);
        
        _activeWindowService.RestoreFocusToLastWindow();
        await Task.Delay(100);
        
        await PasteTextAsync(username);
    }

    [RelayCommand]
    private async Task InsertPasswordAsync()
    {
        if (SelectedCredential == null) return;

        var credential = await _credentialService.GetByIdAsync(SelectedCredential.Id);
        if (credential == null) return;

        var password = _credentialService.DecryptPassword(credential);
        
        // Record usage
        if (_targetWindow != null)
        {
            await _rankingService.RecordUsageAsync(
                SelectedCredential.Id,
                _targetWindow.ProcessName,
                _targetWindow.WindowTitle,
                UsageType.CopyPassword);
        }
        
        RequestClose?.Invoke(this, EventArgs.Empty);
        await Task.Delay(150);
        
        _activeWindowService.RestoreFocusToLastWindow();
        await Task.Delay(100);
        
        await PasteTextAsync(password);
    }

    [RelayCommand]
    private async Task InsertBothAsync()
    {
        if (SelectedCredential == null) return;

        var credential = await _credentialService.GetByIdAsync(SelectedCredential.Id);
        if (credential == null) return;

        var username = SelectedCredential.Username;
        var password = _credentialService.DecryptPassword(credential);
        
        // Record usage
        if (_targetWindow != null)
        {
            await _rankingService.RecordUsageAsync(
                SelectedCredential.Id,
                _targetWindow.ProcessName,
                _targetWindow.WindowTitle,
                UsageType.AutoFill);
        }
        
        RequestClose?.Invoke(this, EventArgs.Empty);
        await Task.Delay(150);
        
        _activeWindowService.RestoreFocusToLastWindow();
        await Task.Delay(100);
        
        // Paste username, send Tab, paste password
        await PasteTextAsync(username);
        await Task.Delay(100);
        await SendTabAsync();
        await Task.Delay(100);
        await PasteTextAsync(password);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void SelectNext()
    {
        if (Credentials.Count == 0 || SelectedCredential == null) return;
        
        var index = Credentials.IndexOf(SelectedCredential);
        if (index < Credentials.Count - 1)
        {
            SelectedCredential = Credentials[index + 1];
        }
    }

    [RelayCommand]
    private void SelectPrevious()
    {
        if (Credentials.Count == 0 || SelectedCredential == null) return;
        
        var index = Credentials.IndexOf(SelectedCredential);
        if (index > 0)
        {
            SelectedCredential = Credentials[index - 1];
        }
    }

    /// <summary>
    /// Pastes text using clipboard + Ctrl+V via keybd_event (Win32 API).
    /// </summary>
    private static async Task PasteTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        // Set text to clipboard
        Clipboard.SetText(text);
        await Task.Delay(100);
        
        // Simulate Ctrl+V using keybd_event (Win32 API)
        // Press Ctrl
        NativeMethods.keybd_event(NativeMethods.VK_CONTROL, 0, 0, UIntPtr.Zero);
        await Task.Delay(30);
        // Press V
        NativeMethods.keybd_event(NativeMethods.VK_V, 0, 0, UIntPtr.Zero);
        await Task.Delay(30);
        // Release V
        NativeMethods.keybd_event(NativeMethods.VK_V, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        await Task.Delay(30);
        // Release Ctrl
        NativeMethods.keybd_event(NativeMethods.VK_CONTROL, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        
        await Task.Delay(100);
    }
    
    /// <summary>
    /// Simulates Tab key press using keybd_event.
    /// </summary>
    private static async Task SendTabAsync()
    {
        const byte VK_TAB = 0x09;
        
        NativeMethods.keybd_event(VK_TAB, 0, 0, UIntPtr.Zero);
        await Task.Delay(30);
        NativeMethods.keybd_event(VK_TAB, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        await Task.Delay(50);
    }
}
