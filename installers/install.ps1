# NetworkDriveManager Windows Installer
# Run as Administrator: powershell -ExecutionPolicy Bypass -File install.ps1

#Requires -RunAsAdministrator

param(
    [string]$InstallDir = "$env:ProgramFiles\NetworkDriveManager"
)

$ErrorActionPreference = 'Stop'
$AppName = "NetworkDriveManager"
$ExeName = "NetworkDriveManager.exe"

Write-Host ""
Write-Host "=== $AppName Installer ===" -ForegroundColor Cyan
Write-Host ""

# Determine source directory (same directory as this script)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not (Test-Path (Join-Path $ScriptDir $ExeName))) {
    Write-Host "Error: $ExeName not found in $ScriptDir" -ForegroundColor Red
    Write-Host "Please run this script from the extracted archive directory." -ForegroundColor Yellow
    exit 1
}

# Create installation directory
Write-Host "Installing to: $InstallDir"
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

# Copy application files
Write-Host "Copying files..."
Copy-Item -Path (Join-Path $ScriptDir "*") -Destination $InstallDir -Recurse -Force -Exclude "install.ps1", "uninstall.ps1"

# Create Start Menu shortcut
$StartMenuPath = Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs\$AppName.lnk"
Write-Host "Creating Start Menu shortcut..."
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($StartMenuPath)
$Shortcut.TargetPath = Join-Path $InstallDir $ExeName
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.Description = "Network Drive Manager"
$Shortcut.Save()

# Create Desktop shortcut
$DesktopPath = Join-Path ([Environment]::GetFolderPath("CommonDesktopDirectory")) "$AppName.lnk"
Write-Host "Creating Desktop shortcut..."
$Shortcut = $WshShell.CreateShortcut($DesktopPath)
$Shortcut.TargetPath = Join-Path $InstallDir $ExeName
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.Description = "Network Drive Manager"
$Shortcut.Save()

# Copy uninstaller
Copy-Item -Path (Join-Path $ScriptDir "uninstall.ps1") -Destination $InstallDir -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Installation complete!" -ForegroundColor Green
Write-Host "You can launch $AppName from the Start Menu or Desktop shortcut." -ForegroundColor Cyan
Write-Host ""
