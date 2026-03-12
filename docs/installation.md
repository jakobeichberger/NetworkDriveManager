---
layout: default
title: Installation
---

# Installation

Network Drive Manager is distributed as a **self-contained** executable — no separate .NET runtime installation is required. Each release archive includes the application binary and an installer script for your platform.

## Requirements

| Requirement | Details |
|---|---|
| **Windows** | Windows 10 or later (x64) |
| **macOS** | macOS 12 (Monterey) or later (Intel or Apple Silicon) |
| **Linux** | x64 distribution with `cifs-utils` installed (auto-installed by the installer script) |
| **.NET SDK** (build from source only) | 10.0 or later |

## Download

Download the latest release for your platform from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.

| Platform | Archive | Installer |
|----------|---------|-----------|
| Windows (x64) | `NetworkDriveManager-windows-x64.zip` | `install.ps1` |
| macOS (Intel) | `NetworkDriveManager-macos-x64.tar.gz` | `install-macos.sh` |
| macOS (Apple Silicon) | `NetworkDriveManager-macos-arm64.tar.gz` | `install-macos.sh` |
| Linux (x64) | `NetworkDriveManager-linux-x64.tar.gz` | `install-linux.sh` |

## Windows

1. Download `NetworkDriveManager-windows-x64.zip` from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.
2. Extract the archive.
3. Run the installer (creates Start Menu and Desktop shortcuts):

   ```powershell
   # Open PowerShell as Administrator and navigate to the extracted folder
   powershell -ExecutionPolicy Bypass -File install.ps1
   ```

4. Launch **Network Drive Manager** from the Start Menu or Desktop.

### Uninstall (Windows)

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Program Files\NetworkDriveManager\uninstall.ps1"
```

## macOS

1. Download `NetworkDriveManager-macos-arm64.tar.gz` (Apple Silicon) or `NetworkDriveManager-macos-x64.tar.gz` (Intel) from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.
2. Extract and install:

   ```bash
   tar -xzf NetworkDriveManager-macos-*.tar.gz
   cd NetworkDriveManager-macos-*
   sudo bash install-macos.sh
   ```

3. Launch from `/Applications/NetworkDriveManager/` or run `networkdrivemanager` in the terminal.

### Uninstall (macOS)

```bash
sudo bash /Applications/NetworkDriveManager/uninstall-macos.sh
```

## Linux

1. Download `NetworkDriveManager-linux-x64.tar.gz` from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.
2. Extract and install (the installer automatically installs required system dependencies):

   ```bash
   tar -xzf NetworkDriveManager-linux-x64.tar.gz
   cd NetworkDriveManager-linux-x64
   sudo bash install-linux.sh
   ```

3. Launch from your application menu or run `networkdrivemanager` in the terminal.

The Linux installer automatically installs required dependencies (`cifs-utils` and GUI libraries) on Debian/Ubuntu, Fedora/RHEL, Arch, and openSUSE systems.

### Uninstall (Linux)

```bash
sudo bash /opt/NetworkDriveManager/uninstall-linux.sh
```

## Build from Source

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download).

2. Clone the repository:

   ```bash
   git clone https://github.com/jakobeichberger/NetworkDriveManager.git
   cd NetworkDriveManager
   ```

3. Build and run:

   ```bash
   dotnet run --project NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj
   ```

4. To publish a self-contained executable for your platform:

   ```bash
   # Windows
   dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj \
     -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

   # macOS (Apple Silicon)
   dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj \
     -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true

   # macOS (Intel)
   dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj \
     -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true

   # Linux
   dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj \
     -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
   ```
