param(
    [ValidateSet("start", "stop", "restart")]
    [string]$Action = "start"
)

$root = $PSScriptRoot
$pidFile = Join-Path $root "stack-pids.json"

function Start-Stack {
    Write-Host "Starting API, Blazor, WPF, MAUI..." -ForegroundColor Cyan
    $procs = @()

    $procs += Start-Process "dotnet" -ArgumentList "run","--project","PasswordManager.Api" -WorkingDirectory $root -PassThru
    $procs += Start-Process "dotnet" -ArgumentList "run","--project","PasswordManager.Blazor" -WorkingDirectory $root -PassThru
    $procs += Start-Process "dotnet" -ArgumentList "run","--project","src/PasswordManager/PasswordManager.csproj" -WorkingDirectory $root -PassThru
    $procs += Start-Process "dotnet" -ArgumentList "run","--project","src/PasswordManager.Maui/PasswordManager.Maui.csproj","-f","net10.0-windows10.0.19041.0" -WorkingDirectory $root -PassThru

    $data = $procs | Select-Object Id, ProcessName
    $data | ConvertTo-Json | Set-Content -Encoding UTF8 $pidFile
    Write-Host "Stack started. PIDs saved to $pidFile" -ForegroundColor Green
}

function Stop-Stack {
    if (-not (Test-Path $pidFile)) {
        Write-Host "No stack-pids.json file found. Nothing to stop." -ForegroundColor Yellow
        return
    }

    $items = Get-Content $pidFile -Raw | ConvertFrom-Json
    foreach ($p in $items) {
        try {
            Write-Host "Stopping PID $($p.Id) ($($p.ProcessName))" -ForegroundColor Yellow
            Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
        }
        catch {
        }
    }

    Remove-Item $pidFile -ErrorAction SilentlyContinue
    Write-Host "Stack stopped." -ForegroundColor Green
}

switch ($Action) {
    "start"   { Start-Stack }
    "stop"    { Stop-Stack }
    "restart" { Stop-Stack; Start-Sleep -Seconds 1; Start-Stack }
}

