using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using PasswordManager.Services;
using PasswordManager.ViewModels;
using Application = System.Windows.Application;

namespace PasswordManager.Views;

public partial class MainWindow : Window
{
    private TaskbarIcon? _taskbarIcon;
    private bool _isExiting;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += OnLoaded;
        StateChanged += OnStateChanged;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsVm = App.GetService<SettingsViewModel>();
        var settingsWindow = new SettingsWindow(settingsVm)
        {
            Owner = this
        };
        
        settingsWindow.HotkeyChanged += OnHotkeyChanged;
        settingsWindow.ShowDialog();
    }

    private void OnHotkeyChanged(object? sender, HotkeyChangedEventArgs e)
    {
        var hotkeyService = App.GetService<IGlobalHotkeyService>();
        hotkeyService.Unregister();
        
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        hotkeyService.Register(hwnd, e.Modifiers, e.Key);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        App.RegisterGlobalHotkey(this);
        App.SetUnlocked(true);
        SetupSystemTray();
    }

    private void SetupSystemTray()
    {
        _taskbarIcon = new TaskbarIcon
        {
            ToolTipText = "Password Manager (Ctrl+Alt+P для быстрой вставки)"
        };

        // Create context menu
        var contextMenu = new System.Windows.Controls.ContextMenu();
        
        var showItem = new System.Windows.Controls.MenuItem { Header = "Показать" };
        showItem.Click += (s, e) => ShowWindow();
        
        var exitItem = new System.Windows.Controls.MenuItem { Header = "Выход" };
        exitItem.Click += (s, e) => ExitApplication();
        
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(new System.Windows.Controls.Separator());
        contextMenu.Items.Add(exitItem);
        
        _taskbarIcon.ContextMenu = contextMenu;
        _taskbarIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            _taskbarIcon?.ShowBalloonTip(
                "Password Manager",
                "Приложение свёрнуто в трей. Нажмите Ctrl+Alt+P для быстрой вставки.",
                BalloonIcon.Info);
        }
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        _taskbarIcon?.Dispose();
        Application.Current.Shutdown();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            return;
        }
        
        App.SetUnlocked(false);
        base.OnClosing(e);
    }
}
