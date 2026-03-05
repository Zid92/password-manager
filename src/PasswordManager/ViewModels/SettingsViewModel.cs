using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PasswordManager.Data;
using PasswordManager.Models;

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
            var startWithWindows = await _databaseService.GetSettingAsync(SettingsKeys.StartWithWindows);
            StartWithWindows = startWithWindows == "true";

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
            ErrorMessage = $"Ошибка сохранения: {ex.Message}";
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
            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (key == null) return;

            if (enable && exePath != null)
            {
                key.SetValue(appName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(appName, false);
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
