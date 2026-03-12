# How to run and use Password Manager

## Prerequisites

- Installed **.NET 10 SDK** ([download](https://dotnet.microsoft.com/download)).
- Repository checked out locally, for example in `c:\Users\User\source\repos\pm`.
- For **MAUI** on Windows: MAUI workload installed. If MAUI fails to build, run once:

```bash
dotnet workload install maui
dotnet workload restore src/PasswordManager.Maui/PasswordManager.Maui.csproj
```

This may take several minutes. If the machine requested a reboot during installation, restart and run the commands again if needed.

---

## Step 1. Start the API (required)

The API is the server. Without it, Web, WPF and MAUI clients cannot work.

From the repository root, run:

```bash
dotnet run --project PasswordManager.Api
```

Keep this window open. In the console you should see something like:

```text
Now listening on: http://localhost:5131
```

By default all clients are configured to talk to **http://localhost:5131** (see `PasswordManager.Api/Properties/launchSettings.json`).

You can also use a helper script (from repo root):

- **PowerShell:** `.\stack.ps1 start` (starts API + Blazor + WPF + MAUI)
- To stop everything: `.\stack.ps1 stop`
- To restart everything: `.\stack.ps1 restart`

---

## Step 2. Start a client

In a **new** terminal (still from the repo root) start any of the clients.

### Option A: Web (Blazor)

```bash
dotnet run --project PasswordManager.Blazor
```

In the console you will see something like:

```text
Now listening on: http://localhost:5085
```

Open this URL in your browser (for example `http://localhost:5085`). The Blazor app will use the API URL from `PasswordManager.Blazor/wwwroot/appsettings.json` (`ApiBaseUrl`, by default `http://localhost:5131`).

### Option B: WPF (Windows)

```bash
dotnet run --project src/PasswordManager/PasswordManager.csproj
```

The WPF desktop window will open. It also uses the central API (no local vault).

### Option C: MAUI (Windows)

```bash
dotnet run --project src/PasswordManager.Maui/PasswordManager.Maui.csproj -f net10.0-windows10.0.19041.0
```

This will open the MAUI desktop app on Windows, also talking to the same API.

You can run several clients at the same time (Blazor, WPF and MAUI). All of them share the same server-side database and master password.

---

## Step 3. First login and working with passwords

1. **Unlock the vault**  
   In any client, enter a master password and click **Unlock**.  
   If the server does not yet have a vault, it will be created on first successful unlock. The same master password must be used in all clients.

2. **Add a password**  
   - In Blazor: go to **Vault → New credential** and fill in **Title, Username, Password** and optionally **URL, Notes**, then click **Save**.  
   - In WPF/MAUI: click the **+ / Add** button, fill in the same fields and save.

3. **View and copy passwords**  
   Entries appear in the main list. Each item usually has actions like **Copy user / Copy pass** or **Show password**, which copy the value to the clipboard.

4. **Edit and delete**  
   Use **Edit** and **Delete** buttons on the entry card to modify or remove a credential.

5. **Search**  
   Use the search box at the top of the list. Typing filters entries by title, username, or URL.

6. **Breach check**  
   In MAUI/WPF there is a **Check** / **Breach check** button in the toolbar. It runs a Have I Been Pwned check for all passwords. Compromised passwords are highlighted in the list.

---

## If the API is not on localhost:5131

If the API runs on a different host/port, configure the same URL in all clients:

| Client | Where to change |
|--------|-----------------|
| **Blazor** | `PasswordManager.Blazor/wwwroot/appsettings.json` → `ApiBaseUrl` |
| **WPF** | `src/PasswordManager/appsettings.json` → `ApiBaseUrl` |
| **MAUI** | `src/PasswordManager.Maui/MauiProgram.cs` → constant `ApiBaseUrl` |

Example: if the API prints `Now listening on: http://localhost:6000`, then `ApiBaseUrl` should be `http://localhost:6000` in all three places. For access from another machine on the network, use e.g. `http://192.168.1.10:5131`.

---

## Typical startup order

1. Start the **API**: `dotnet run --project PasswordManager.Api` (or `.\stack.ps1 start`).  
2. Start the desired **clients**: Blazor, WPF and/or MAUI.  
3. In a client, enter the master password and work with the shared vault.

Stopping the API makes the vault temporarily unavailable until the API is started again.

