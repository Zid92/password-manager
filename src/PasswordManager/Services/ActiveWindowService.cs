using System.Diagnostics;
using System.Text;
using PasswordManager.Native;

namespace PasswordManager.Services;

public class ActiveWindowService : IActiveWindowService
{
    private IntPtr _lastWindowHandle;

    public ActiveWindowInfo GetActiveWindowInfo()
    {
        var handle = NativeMethods.GetForegroundWindow();
        _lastWindowHandle = handle;

        var info = new ActiveWindowInfo { Handle = handle };

        // Get process info
        NativeMethods.GetWindowThreadProcessId(handle, out uint processId);
        info.ProcessId = processId;

        try
        {
            var process = Process.GetProcessById((int)processId);
            info.ProcessName = process.ProcessName;
        }
        catch
        {
            info.ProcessName = "Unknown";
        }

        // Get window title
        var length = NativeMethods.GetWindowTextLength(handle);
        if (length > 0)
        {
            var sb = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(handle, sb, sb.Capacity);
            info.WindowTitle = sb.ToString();
        }

        return info;
    }

    public void RestoreFocusToLastWindow()
    {
        if (_lastWindowHandle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(_lastWindowHandle);
        }
    }
}
