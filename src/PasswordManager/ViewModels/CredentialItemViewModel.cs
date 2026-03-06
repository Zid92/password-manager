using CommunityToolkit.Mvvm.ComponentModel;
using PasswordManager.Core.Models;

namespace PasswordManager.ViewModels;

public partial class CredentialItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string? _url;

    [ObservableProperty]
    private bool _isBreached;

    [ObservableProperty]
    private DateTime _updatedAt;

    public static CredentialItemViewModel FromModel(Credential credential)
    {
        return new CredentialItemViewModel
        {
            Id = credential.Id,
            Title = credential.Title,
            Username = credential.Username,
            Url = credential.Url,
            IsBreached = credential.IsBreached,
            UpdatedAt = credential.UpdatedAt
        };
    }
}
