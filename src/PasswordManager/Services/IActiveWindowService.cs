namespace PasswordManager.Services;

public interface IActiveWindowService
{
    ActiveWindowInfo GetActiveWindowInfo();
    void RestoreFocusToLastWindow();
}

public class ActiveWindowInfo
{
    public IntPtr Handle { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public uint ProcessId { get; set; }
}
