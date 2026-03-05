namespace PasswordManager.Services;

public interface INavigationService
{
    void NavigateToMain();
    void NavigateToLogin();
    void CloseCurrentWindow();
}
