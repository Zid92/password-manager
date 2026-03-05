using System.Windows;
using System.Windows.Input;
using PasswordManager.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PasswordManager.Views;

public partial class QuickInsertWindow : Window
{
    private readonly QuickInsertViewModel _viewModel;

    public QuickInsertWindow(QuickInsertViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.RequestClose += (s, e) => Close();

        Loaded += OnLoaded;
        Deactivated += OnDeactivated;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadCredentialsAsync();
        SearchBox.Focus();
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
        base.OnPreviewKeyDown(e);
    }
}
