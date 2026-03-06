using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PasswordManager.Core.Data;
using PasswordManager.Core.Models;

namespace PasswordManager.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private string _hotkeyDisplay = "Ctrl + Alt + P";

    [ObservableProperty]
    private ModifierKeys _selectedModifiers = ModifierKeys.Control | ModifierKeys.Alt;

    [ObservableProperty]
    private Key _selectedKey = Key.P;

    public event EventHandler? RequestClose;
    public event EventHandler<HotkeyChangedEventArgs>? HotkeyChanged;

    public SettingsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            // Check ACTUAL registry state for startup setting (not just database)
            StartWithWindows = IsStartupEnabled();

            var minimizeToTray = await _databaseService.GetSettingAsync(SettingsKeys.MinimizeToTray);
            MinimizeToTray = minimizeToTray != "false";

            // Load hotkey settings
            var modifiers = await _databaseService.GetSettingAsync(SettingsKeys.HotkeyModifiers);
            var key = await _databaseService.GetSettingAsync(SettingsKeys.HotkeyKey);

            if (!string.IsNullOrEmpty(modifiers) && Enum.TryParse<ModifierKeys>(modifiers, out var mod))
            {
                SelectedModifiers = mod;
            }

            if (!string.IsNullOrEmpty(key) && Enum.TryParse<Key>(key, out var k))
            {
                SelectedKey = k;
            }

            UpdateHotkeyDisplay();
        }
        catch
        {
            // Use defaults
        }
    }

    private static bool IsStartupEnabled()
    {
        const string appName = "PasswordManager";
        
        try
        {
            // Check if entry exists in Run key
            using var runKey = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            
            if (runKey == null) return false;
            
            var runValue = runKey.GetValue(appName);
            if (runValue == null) return false;
            
            // Check if disabled via Task Manager (StartupApproved\Run)
            using var approvedKey = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", false);
            
            if (approvedKey != null)
            {
                var approvedValue = approvedKey.GetValue(appName) as byte[];
                if (approvedValue != null && approvedValue.Length > 0)
                {
                    // First byte: 02 = enabled, 03 = disabled by user
                    // Values 00, 01, 06 also indicate disabled states
                    if (approvedValue[0] == 0x03 || approvedValue[0] == 0x00 || 
                        approvedValue[0] == 0x01 || approvedValue[0] == 0x06)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateHotkeyDisplay()
    {
        var parts = new List<string>();
        
        if (SelectedModifiers.HasFlag(ModifierKeys.Control))
            parts.Add("Ctrl");
        if (SelectedModifiers.HasFlag(ModifierKeys.Alt))
            parts.Add("Alt");
        if (SelectedModifiers.HasFlag(ModifierKeys.Shift))
            parts.Add("Shift");
        if (SelectedModifiers.HasFlag(ModifierKeys.Windows))
            parts.Add("Win");
        
        parts.Add(SelectedKey.ToString());
        
        HotkeyDisplay = string.Join(" + ", parts);
    }

    public void SetNewHotkey(ModifierKeys modifiers, Key key)
    {
        SelectedModifiers = modifiers;
        SelectedKey = key;
        UpdateHotkeyDisplay();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // Save settings to database
            await _databaseService.SetSettingAsync(SettingsKeys.StartWithWindows, StartWithWindows.ToString().ToLower());
            await _databaseService.SetSettingAsync(SettingsKeys.MinimizeToTray, MinimizeToTray.ToString().ToLower());
            await _databaseService.SetSettingAsync(SettingsKeys.HotkeyModifiers, SelectedModifiers.ToString());
            await _databaseService.SetSettingAsync(SettingsKeys.HotkeyKey, SelectedKey.ToString());

            // Apply startup setting
            SetStartupWithWindows(StartWithWindows);

            // Notify about hotkey change
            HotkeyChanged?.Invoke(this, new HotkeyChangedEventArgs(SelectedModifiers, SelectedKey));

            RequestClose?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving settings: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private static void SetStartupWithWindows(bool enable)
    {
        const string appName = "PasswordManager";
        var exePath = Environment.ProcessPath;

        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (runKey == null) return;

            if (enable && exePath != null)
            {
                // Add to Run key
                runKey.SetValue(appName, $"\"{exePath}\"");
                
                // Enable in StartupApproved (remove disabled state)
                try
                {
                    using var approvedKey = Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", true);
                    
                    if (approvedKey != null)
                    {
                        // Set to enabled: first byte = 0x02, rest = timestamp (can be zeros)
                        byte[] enabledValue = new byte[12];
                        enabledValue[0] = 0x02;
                        approvedKey.SetValue(appName, enabledValue, RegistryValueKind.Binary);
                    }
                }
                catch
                {
                    // StartupApproved key might not exist, that's OK
                }
            }
            else
            {
                // Remove from Run key
                runKey.DeleteValue(appName, false);
                
                // Also remove from StartupApproved
                try
                {
                    using var approvedKey = Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", true);
                    
                    approvedKey?.DeleteValue(appName, false);
                }
                catch
                {
                    // Ignore
                }
            }
        }
        catch
        {
            // Ignore registry errors
        }
    }
}

public class HotkeyChangedEventArgs : EventArgs
{
    public ModifierKeys Modifiers { get; }
    public Key Key { get; }

    public HotkeyChangedEventArgs(ModifierKeys modifiers, Key key)
    {
        Modifiers = modifiers;
        Key = key;
    }
}
