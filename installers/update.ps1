# NetworkDriveManager Auto-Update Script (Windows)
# Downloads and installs the latest version from GitHub Releases.
# Usage: powershell -ExecutionPolicy Bypass -File update.ps1

param(
    [string]$InstallDir = "$env:ProgramFiles\NetworkDriveManager"
)

$ErrorActionPreference = 'Stop'
$AppName = "NetworkDriveManager"
$ExeName = "NetworkDriveManager.exe"
$RepoOwner = "jakobeichberger"
$RepoName = "NetworkDriveManager"
$AssetPattern = "NetworkDriveManager-Setup-*.exe"

Write-Host ""
Write-Host "=== $AppName Auto-Updater ===" -ForegroundColor Cyan
Write-Host ""

# Get current version
$CurrentVersion = "0.0.0"
$ExePath = Join-Path $InstallDir $ExeName
if (Test-Path $ExePath) {
    $VersionInfo = (Get-Item $ExePath).VersionInfo
    $CurrentVersion = $VersionInfo.FileVersion
    if (-not $CurrentVersion) { $CurrentVersion = "0.0.0" }
}
Write-Host "Current version: $CurrentVersion"

# Fetch latest release from GitHub
Write-Host "Checking for updates..."
try {
    $ApiUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/latest"
    $Headers = @{ "User-Agent" = "NetworkDriveManager-Updater" }
    $Release = Invoke-RestMethod -Uri $ApiUrl -Headers $Headers -TimeoutSec 30
} catch {
    Write-Host "Error: Could not check for updates. $_" -ForegroundColor Red
    exit 1
}

$LatestVersion = $Release.tag_name -replace '^v', ''
Write-Host "Latest version:  $LatestVersion"

# Compare versions
if ([version]$LatestVersion -le [version]$CurrentVersion) {
    Write-Host ""
    Write-Host "You are already running the latest version." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "New version available: $LatestVersion" -ForegroundColor Yellow

# Find the installer asset (prefer Setup exe, fall back to zip)
$SetupAsset = $Release.assets | Where-Object { $_.name -match "NetworkDriveManager-Setup-.*\.exe$" } | Select-Object -First 1
$ZipAsset = $Release.assets | Where-Object { $_.name -match "NetworkDriveManager-windows-x64\.zip$" } | Select-Object -First 1

$UseInstaller = $false
$DownloadAsset = $null

if ($SetupAsset) {
    $DownloadAsset = $SetupAsset
    $UseInstaller = $true
    Write-Host "Found installer: $($SetupAsset.name)"
} elseif ($ZipAsset) {
    $DownloadAsset = $ZipAsset
    Write-Host "Found archive: $($ZipAsset.name)"
} else {
    Write-Host "Error: No compatible download found in the latest release." -ForegroundColor Red
    exit 1
}

# Download the asset
$TempDir = Join-Path $env:TEMP "NetworkDriveManager_Update"
if (Test-Path $TempDir) { Remove-Item -Path $TempDir -Recurse -Force }
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

$DownloadPath = Join-Path $TempDir $DownloadAsset.name
Write-Host "Downloading $($DownloadAsset.name)..."
Invoke-WebRequest -Uri $DownloadAsset.browser_download_url -OutFile $DownloadPath -TimeoutSec 120

if ($UseInstaller) {
    # Run the installer silently
    Write-Host "Running installer..."
    Start-Process -FilePath $DownloadPath -ArgumentList "/SILENT", "/SUPPRESSMSGBOXES", "/NORESTART" -Wait
} else {
    # Extract the zip and copy files
    Write-Host "Extracting update..."
    Expand-Archive -Path $DownloadPath -DestinationPath $TempDir -Force

    # Stop the running application
    $RunningProcess = Get-Process -Name $AppName -ErrorAction SilentlyContinue
    if ($RunningProcess) {
        Write-Host "Stopping $AppName..."
        $RunningProcess | Stop-Process -Force
        Start-Sleep -Seconds 2
    }

    # Copy updated files
    Write-Host "Installing update to $InstallDir..."
    $ExtractedDir = Get-ChildItem -Path $TempDir -Directory | Select-Object -First 1
    if ($ExtractedDir) {
        Copy-Item -Path "$($ExtractedDir.FullName)\*" -Destination $InstallDir -Recurse -Force
    }
}

# Cleanup
Write-Host "Cleaning up..."
Remove-Item -Path $TempDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Update to version $LatestVersion complete!" -ForegroundColor Green
Write-Host ""
