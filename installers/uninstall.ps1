# NetworkDriveManager Windows Uninstaller
# Run as Administrator: powershell -ExecutionPolicy Bypass -File uninstall.ps1

#Requires -RunAsAdministrator

param(
    [string]$InstallDir = "$env:ProgramFiles\NetworkDriveManager"
)

$ErrorActionPreference = 'Stop'
$AppName = "NetworkDriveManager"

Write-Host ""
Write-Host "=== $AppName Uninstaller ===" -ForegroundColor Cyan
Write-Host ""

# Remove Start Menu shortcut
$StartMenuPath = Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs\$AppName.lnk"
if (Test-Path $StartMenuPath) {
    Remove-Item $StartMenuPath -Force
    Write-Host "Removed Start Menu shortcut."
}

# Remove Desktop shortcut
$DesktopPath = Join-Path ([Environment]::GetFolderPath("CommonDesktopDirectory")) "$AppName.lnk"
if (Test-Path $DesktopPath) {
    Remove-Item $DesktopPath -Force
    Write-Host "Removed Desktop shortcut."
}

# Remove installation directory
if (Test-Path $InstallDir) {
    Remove-Item -Path $InstallDir -Recurse -Force
    Write-Host "Removed installation directory: $InstallDir"
}

Write-Host ""
Write-Host "Uninstallation complete." -ForegroundColor Green
Write-Host ""
