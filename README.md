# Password Manager

A secure, cross-platform password manager with biometric authentication and breach checking. **All clients (Web, WPF, MAUI) use a central HTTP API** — one vault on the server.

![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)
![Windows](https://img.shields.io/badge/Platform-Windows-blue)
![Android](https://img.shields.io/badge/Platform-Android-green)
![iOS](https://img.shields.io/badge/Platform-iOS-lightgray)
![macOS](https://img.shields.io/badge/Platform-macOS-lightgray)
![Build](https://img.shields.io/github/actions/workflow/status/Zid92/password-manager/ci.yml?branch=master)
![License](https://img.shields.io/badge/License-MIT-green)

## Architecture

- **PasswordManager.Api** — ASP.NET Core HTTP API (central server). Stores the encrypted vault (SQLCipher), handles open/lock, credentials CRUD, breach check.
- **PasswordManager.Blazor** — Blazor WebAssembly frontend (browser).
- **PasswordManager (WPF)** — Windows desktop client.
- **PasswordManager.Maui** — Cross-platform client (Android, iOS, macOS, Windows).

All clients talk to the same API; there is no local vault. The server holds a single vault per deployment.

## Features

- **Secure Storage** — Passwords encrypted with AES-256, database protected by SQLCipher (on server)
- **Master Password** — Single password to access the vault (sent to API for open/lock)
- **Breach Detection** — Integration with [Have I Been Pwned](https://haveibeenpwned.com/) to check for compromised passwords
- **Cross-Platform** — Web (Blazor), Windows WPF, and MAUI (Android, iOS, macOS, Windows)
- **Global Hotkeys** — Quick credential insertion (Windows WPF: `Ctrl+Alt+P`)
- **Material Design** — Modern UI with light/dark theme support

## Project Structure

```
PasswordManager/
├── src/
│   ├── PasswordManager.Core/       # Shared library (models, services)
│   ├── PasswordManager/            # WPF desktop (Windows)
│   ├── PasswordManager.Maui/       # MAUI (Android, iOS, macOS, Windows)
│   └── (API lives in repo root)
├── PasswordManager.Api/             # ASP.NET Core HTTP API (central server)
├── PasswordManager.Blazor/         # Blazor WebAssembly (web UI)
├── PasswordManager.Contracts/      # Shared DTOs for API
├── PasswordManager.ApiClient/      # API-backed IDatabaseService / ICredentialService
├── tests/PasswordManager.Tests/
└── PasswordManager.slnx
```

## How to run

### 1. Start the API (required)

From the repository root:

```bash
dotnet run --project PasswordManager.Api
```

Leave this running. By default it listens on **https://localhost:5001** (see output for the exact URL).

### 2. Run a client

**Web (Blazor):**

```bash
dotnet run --project PasswordManager.Blazor
```

Open the URL shown in the console (e.g. `http://localhost:5180`). The Blazor app uses the API URL from `PasswordManager.Blazor/wwwroot/appsettings.json` (`ApiBaseUrl`, default `https://localhost:5001`).

**Windows WPF:**

```bash
dotnet run --project src/PasswordManager/PasswordManager.csproj
```

API base URL is read from `src/PasswordManager/appsettings.json` (`ApiBaseUrl`).

**MAUI (e.g. Windows):**

```bash
dotnet run --project src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-windows10.0.19041.0
```

API base URL is set in `MauiProgram.cs` (`ApiBaseUrl` constant).

### 3. First use

- Open the **Blazor** app in the browser (or any client).
- If the server vault is new, open the vault with a master password (this creates/opens the vault on the server).
- Add credentials and use the app as usual. All clients see the same vault while the API is running.

## Requirements

- .NET 10 SDK
- For MAUI: corresponding workload (e.g. `dotnet workload install maui`)

## Configuration

| Place | Setting | Description |
|-------|---------|-------------|
| `PasswordManager.Blazor/wwwroot/appsettings.json` | `ApiBaseUrl` | API base URL for Blazor (default `https://localhost:5001`) |
| `src/PasswordManager/appsettings.json` | `ApiBaseUrl` | API base URL for WPF |
| `src/PasswordManager.Maui/MauiProgram.cs` | `ApiBaseUrl` | API base URL for MAUI (constant) |

## Testing

```bash
dotnet test tests/PasswordManager.Tests/PasswordManager.Tests.csproj
```

Or from solution root:

```bash
dotnet test
```

(Some targets may fail if Android SDK or workload is missing; the core test project builds and runs on Windows.)

## Build from source (publish)

```bash
# API (for deployment)
dotnet publish PasswordManager.Api/PasswordManager.Api.csproj -c Release -o ./publish-api

# Blazor (static files + API or host elsewhere)
dotnet publish PasswordManager.Blazor/PasswordManager.Blazor.csproj -c Release -o ./publish-blazor

# WPF Windows
dotnet publish src/PasswordManager/PasswordManager.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# MAUI Android
dotnet publish src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true
```

## Security

- Passwords are encrypted (AES-256) and stored in SQLCipher on the **server**.
- Master password is sent to the API only for open/lock (HTTPS recommended).
- Breach checking uses k-anonymity (only first 5 hash characters sent to HIBP).

## License

MIT License. See [LICENSE](LICENSE) file.
