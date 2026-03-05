using System.Windows.Input;

namespace PasswordManager.Services;

public interface IGlobalHotkeyService
{
    event EventHandler? HotkeyPressed;
    void Register(IntPtr windowHandle, ModifierKeys modifiers, Key key);
    void Unregister();
    void ProcessHotkeyMessage(IntPtr wParam);
}
