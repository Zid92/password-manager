using SQLite;

namespace PasswordManager.Core.Models;

public class AppSettings
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;
    
    public string Value { get; set; } = string.Empty;
}

public static class SettingsKeys
{
    public const string MasterPasswordHash = "MasterPasswordHash";
    public const string Salt = "Salt";
    public const string WindowsHelloEnabled = "WindowsHelloEnabled";
    public const string AutoLockMinutes = "AutoLockMinutes";
    public const string HotkeyModifiers = "HotkeyModifiers";
    public const string HotkeyKey = "HotkeyKey";
    public const string DarkMode = "DarkMode";
    public const string StartWithWindows = "StartWithWindows";
    public const string MinimizeToTray = "MinimizeToTray";
    public const string BiometricEnabled = "BiometricEnabled";
}
