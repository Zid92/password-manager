using System.Windows;
using System.Windows.Input;
using PasswordManager.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PasswordManager.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public event EventHandler<HotkeyChangedEventArgs>? HotkeyChanged;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.RequestClose += (s, e) => Close();
        _viewModel.HotkeyChanged += (s, e) => HotkeyChanged?.Invoke(this, e);
    }

    private void HotkeyInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore modifier keys alone
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin)
        {
            return;
        }

        var modifiers = Keyboard.Modifiers;

        // Require at least one modifier
        if (modifiers == ModifierKeys.None)
        {
            return;
        }

        _viewModel.SetNewHotkey(modifiers, key);
    }
}
