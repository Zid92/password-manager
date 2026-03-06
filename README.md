# Password Manager

A secure, cross-platform password manager with global hotkey support and intelligent credential ranking.

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![Windows](https://img.shields.io/badge/Platform-Windows-blue)
![iOS](https://img.shields.io/badge/Platform-iOS-lightgray)
![Android](https://img.shields.io/badge/Platform-Android-green)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- **Secure Storage** — Passwords encrypted with AES-256, database protected by SQLCipher
- **Master Password** — Single password to access your vault
- **Biometric Authentication** — Windows Hello, Face ID, Touch ID support
- **Global Hotkeys** — Quick credential insertion into any application (Windows: `Ctrl+Alt+P`)
- **Smart Ranking** — Remembers which credentials are used in which applications
- **Breach Detection** — Integration with [Have I Been Pwned](https://haveibeenpwned.com/) to check for compromised passwords
- **System Tray** — Runs in the background, minimizes to system tray (Windows)
- **Cross-Platform** — Native apps for Windows (WPF), iOS, and Android (MAUI)
- **Material Design** — Modern and beautiful UI

## Requirements

### Windows Desktop
- Windows 10/11
- .NET 10 Runtime

### Mobile (iOS/Android)
- iOS 15.0+
- Android 5.0+ (API 21)
- .NET MAUI workload

## Project Structure

```
PasswordManager/
├── src/
│   ├── PasswordManager.Core/     # Shared library (cross-platform)
│   │   ├── Models/               # Data models
│   │   ├── Services/             # Business logic services
│   │   └── Data/                 # Database service
│   ├── PasswordManager/          # WPF Desktop (Windows)
│   │   ├── Services/             # Windows-specific services
│   │   ├── ViewModels/           # WPF ViewModels
│   │   ├── Views/                # WPF Windows
│   │   ├── Converters/           # XAML converters
│   │   └── Native/               # Win32 API interop
│   └── PasswordManager.Maui/     # MAUI Mobile (iOS/Android)
│       ├── Services/             # Platform services
│       ├── ViewModels/           # MAUI ViewModels
│       ├── Views/                # MAUI Pages
│       └── Converters/           # MAUI converters
├── tests/
│   └── PasswordManager.Tests/    # Unit tests (xUnit + Moq)
└── PasswordManager.slnx          # Solution file
```

## Installation

### From Source

```bash
git clone https://github.com/yourusername/password-manager.git
cd password-manager
dotnet build
dotnet run --project src/PasswordManager
```

### Build Release

```bash
dotnet publish src/PasswordManager -c Release -r win-x64 --self-contained
```

## Testing

The project includes comprehensive unit tests using xUnit and Moq.

### Run All Tests

```bash
dotnet test
```

### Run Tests with Detailed Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage

| Category | Tests |
|----------|-------|
| EncryptionService | 19 |
| CredentialService | 13 |
| RankingService | 13 |
| BreachCheckService | 10 |
| Models | 19 |
| ViewModels | 11 |
| Converters | 36 |
| **Total** | **135** |

### Test Structure

- `tests/PasswordManager.Tests/Services/` — Service layer tests with mocked dependencies
- `tests/PasswordManager.Tests/Models/` — Model property and behavior tests
- `tests/PasswordManager.Tests/ViewModels/` — ViewModel mapping and PropertyChanged tests
- `tests/PasswordManager.Tests/Converters/` — XAML converter tests

## Usage

### First Launch

1. Launch the application
2. Create a master password (minimum 8 characters)
3. Optionally enable Windows Hello for quick unlock

### Adding a Password

1. Click the `+` button in the bottom right corner
2. Fill in the fields (title, username, password, URL)
3. Click "Save"

### Quick Insert

1. Open the application where you need to enter credentials
2. Press `Ctrl+Alt+P` (or your configured hotkey)
3. Select the desired entry from the list
4. Press `Enter` to insert username and password
   - `Ctrl+U` — username only
   - `Ctrl+P` — password only

### Checking Passwords for Breaches

1. In the main window, click the shield icon
2. Wait for the check to complete
3. Compromised passwords will be marked with a red badge

## Security

- Passwords are stored in an encrypted SQLite database (SQLCipher)
- AES-256 is used for password encryption
- Master key is derived from the master password using PBKDF2 (100,000 iterations)
- When using Windows Hello, the key is protected by DPAPI and stored in Windows Credential Manager
- Breach checking uses k-anonymity (only the first 5 characters of the hash are sent to the server)

## Technologies

- [.NET 10](https://dotnet.microsoft.com/) — Platform
- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) — Windows Desktop UI
- [.NET MAUI](https://docs.microsoft.com/en-us/dotnet/maui/) — Cross-platform mobile UI
- [MaterialDesignInXAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) — Material Design for WPF
- [CommunityToolkit.Maui](https://github.com/CommunityToolkit/Maui) — MAUI extensions
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) — MVVM framework
- [SQLite + SQLCipher](https://www.zetetic.net/sqlcipher/) — Encrypted database
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) — System tray (Windows)
- [Have I Been Pwned API](https://haveibeenpwned.com/API/v3) — Breach checking
- [xUnit](https://xunit.net/) — Testing framework
- [Moq](https://github.com/moq/moq4) — Mocking library for unit tests

## License

MIT License. See [LICENSE](LICENSE) file.

## Contributing

Pull requests are welcome! For major changes, please open an issue first.
