using System.Windows.Input;
using PasswordManager.Native;

namespace PasswordManager.Services;

public class GlobalHotkeyService : IGlobalHotkeyService
{
    private const int HotkeyId = 9000;
    private IntPtr _windowHandle;
    private bool _isRegistered;

    public event EventHandler? HotkeyPressed;

    public void Register(IntPtr windowHandle, ModifierKeys modifiers, Key key)
    {
        _windowHandle = windowHandle;

        if (_isRegistered)
        {
            Unregister();
        }

        uint nativeModifiers = 0;
        if (modifiers.HasFlag(ModifierKeys.Alt))
            nativeModifiers |= (uint)NativeMethods.KeyModifiers.Alt;
        if (modifiers.HasFlag(ModifierKeys.Control))
            nativeModifiers |= (uint)NativeMethods.KeyModifiers.Control;
        if (modifiers.HasFlag(ModifierKeys.Shift))
            nativeModifiers |= (uint)NativeMethods.KeyModifiers.Shift;
        if (modifiers.HasFlag(ModifierKeys.Windows))
            nativeModifiers |= (uint)NativeMethods.KeyModifiers.Win;

        var vk = (uint)KeyInterop.VirtualKeyFromKey(key);

        _isRegistered = NativeMethods.RegisterHotKey(windowHandle, HotkeyId, nativeModifiers, vk);
    }

    public void Unregister()
    {
        if (_isRegistered && _windowHandle != IntPtr.Zero)
        {
            NativeMethods.UnregisterHotKey(_windowHandle, HotkeyId);
            _isRegistered = false;
        }
    }

    public void ProcessHotkeyMessage(IntPtr wParam)
    {
        if (wParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
