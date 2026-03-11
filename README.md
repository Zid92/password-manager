# Password Manager

A secure, cross-platform password manager with biometric authentication and breach checking.

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![Windows](https://img.shields.io/badge/Platform-Windows-blue)
![Android](https://img.shields.io/badge/Platform-Android-green)
![iOS](https://img.shields.io/badge/Platform-iOS-lightgray)
![macOS](https://img.shields.io/badge/Platform-macOS-lightgray)
![Build](https://img.shields.io/github/actions/workflow/status/Zid92/password-manager/ci.yml?branch=master)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- **Secure Storage** — Passwords encrypted with AES-256, database protected by SQLCipher
- **Master Password** — Single password to access your vault
- **Biometric Authentication** — Windows Hello, Face ID, Touch ID, Fingerprint support
- **Breach Detection** — Integration with [Have I Been Pwned](https://haveibeenpwned.com/) to check for compromised passwords
- **Cross-Platform** — Native apps for Windows, Android, iOS, macOS (MAUI)
- **Global Hotkeys** — Quick credential insertion (Windows WPF only: `Ctrl+Alt+P`)
- **System Tray** — Runs in background (Windows WPF only)
- **Material Design** — Modern UI with light/dark theme support

## Platform Support

| Platform | Status | Notes |
|----------|--------|-------|
| **Android** | ✅ Ready | Download APK from [GitHub Actions](../../actions) |
| **Windows MAUI** | ✅ Ready | Download from [GitHub Actions](../../actions) |
| **Windows WPF** | ✅ Ready | Local build only (global hotkeys, system tray) |
| **iOS** | ⏳ Pending | Requires Xcode 26.2+ (not available on GitHub runners yet) |
| **macOS** | ⏳ Pending | Requires Xcode 26.2+ (not available on GitHub runners yet) |

## Requirements

### Android
- Android 6.0+ (API 23)

### Windows
- Windows 10/11
- .NET 10 Runtime

### iOS / macOS
- iOS 15.0+
- macOS 15.0+ (Mac Catalyst)
- .NET MAUI workload

## Project Structure

```
PasswordManager/
├── src/
│   ├── PasswordManager.Core/      # Shared library (models, services, data access)
│   ├── PasswordManager/           # WPF desktop (Windows only, local build)
│   ├── PasswordManager.Maui/      # MAUI (Android, iOS, macOS, Windows)
│   └── PasswordManager.Api/       # ASP.NET Core HTTP API (central vault server)
├── PasswordManager.Blazor/        # Blazor WebAssembly frontend (web vault UI)
├── PasswordManager.Contracts/     # Shared DTOs for HTTP API
├── tests/
│   └── PasswordManager.Tests/     # Unit tests (xUnit + Moq)
└── PasswordManager.slnx           # Solution file
```

## Installation

### Download Pre-built Apps

1. Go to [GitHub Actions](../../actions)
2. Select the latest successful workflow run
3. Download artifacts:
   - `android-apk-debug` — Android APK
   - `windows-maui-debug` — Windows MAUI app

### Build from Source

```bash
git clone https://github.com/Zid92/password-manager.git
cd password-manager

# Build Android APK
dotnet publish src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true

# Build Windows MAUI
dotnet publish src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-windows10.0.19041.0 -c Debug

# Build Windows WPF (local only, desktop app)
dotnet publish src/PasswordManager/PasswordManager.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# Build iOS (requires Mac with Xcode 26.2+)
dotnet build src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-ios -c Debug

# Build macOS (requires Mac with Xcode 26.2+)
dotnet build src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-maccatalyst -c Debug

# Build and run central HTTP API (backend)
dotnet run --project PasswordManager.Api

# Build and run Blazor Web frontend
dotnet run --project PasswordManager.Blazor
```

The Blazor frontend uses the same HTTP API as MAUI and WPF clients.  
Configure the API base URL for the web app in `PasswordManager.Blazor/wwwroot/appsettings.json` (`ApiBaseUrl`).

## Usage

### First Launch

1. Launch the application
2. Create a master password (minimum 8 characters)
3. Optionally enable biometric unlock (fingerprint / Face ID / Windows Hello)

### Adding a Password

1. Tap the `+` button
2. Fill in the fields:
   - **Title** — Name for this entry (required)
   - **Username** — Login/email
   - **Password** — The password (required)
   - **URL** — Website address (optional)
   - **Notes** — Additional info (optional)
3. Tap **Save**

### Managing Passwords

Each password card shows:
- Title, Username, URL, Notes
- Breach warning if compromised

Action buttons on each card:
- **Copy user** — Copy username to clipboard
- **Copy pass** — Copy password to clipboard
- **Edit** — Modify entry
- **Delete** — Remove entry

### Checking for Breaches

1. Tap **Check** in the toolbar
2. Wait for the check to complete
3. Compromised passwords will show a warning badge

## Testing

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage (summary)

The solution includes comprehensive unit tests for:

- **Core services**: encryption, credentials, ranking, breach-check logic
- **Models**: credential and usage history behaviour
- **UI helpers**: WPF/MVVM converters and view-models
- **HTTP layer**: API contracts and `ApiClient` behaviour (request routing and response parsing)

## Security

- Passwords stored in encrypted SQLite database (SQLCipher)
- AES-256 encryption for individual passwords
- Master key derived using PBKDF2 (100,000 iterations)
- Biometric keys protected by platform secure storage
- Breach checking uses k-anonymity (only first 5 hash characters sent)

## CI/CD

| Workflow | Trigger | Artifacts |
|----------|---------|-----------|
| **Build and Test** | Push/PR to master | Test results |
| **Build Android APK** | Push to master | `android-apk-debug` |
| **Build Windows** | Push to master | `windows-maui-debug` |
| **Build iOS & macOS** | Manual only | Disabled until Xcode 26.2 available |

## Technologies

- [.NET 10](https://dotnet.microsoft.com/) — Platform
- [.NET MAUI](https://docs.microsoft.com/en-us/dotnet/maui/) — Cross-platform UI (Android, iOS, macOS, Windows)
- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/) — Windows Desktop UI (local build)
- [CommunityToolkit.Mvvm](https://docs.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) — MVVM framework
- [CommunityToolkit.Maui](https://github.com/CommunityToolkit/Maui) — MAUI extensions
- [SQLite + SQLCipher](https://www.zetetic.net/sqlcipher/) — Encrypted database
- [Plugin.Fingerprint](https://github.com/smstuebe/xamarin-fingerprint) — Cross-platform biometric auth
- [Have I Been Pwned API](https://haveibeenpwned.com/API/v3) — Breach checking
- [xUnit](https://xunit.net/) + [Moq](https://github.com/moq/moq4) — Testing

## License

MIT License. See [LICENSE](LICENSE) file.

## Contributing

Pull requests are welcome! For major changes, please open an issue first.
