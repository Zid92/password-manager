using CommunityToolkit.Mvvm.ComponentModel;

namespace PasswordManager.ViewModels;

public abstract partial class ViewModelBase : ObservableValidator
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    protected void ClearError() => ErrorMessage = null;
    
    protected void ClearAllErrors()
    {
        ErrorMessage = null;
        ClearErrors();
    }
}
