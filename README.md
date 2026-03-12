# Network Drive Manager

A cross-platform desktop application for managing SMB/CIFS network drive connections, built with **.NET 10** and **Avalonia UI 11.3**. Runs natively on **Windows**, **macOS**, and **Linux** with automatic platform detection.

## Features

- **Cross-platform** — automatically detects Windows, macOS, or Linux and uses the appropriate mount commands
- **Connect & Disconnect** individual drives or all drives at once using a **toggle button** (click once to connect, click again to disconnect)
- **Encrypted Credentials** – username and password are stored using AES-GCM 256-bit encryption
- **Automatic Monitoring** – checks every 30 seconds whether connected drives are still mapped and alerts you if a drive unexpectedly disconnects
- **Server Reachability** – tests SMB (port 445) connectivity before connecting and warns you when a server becomes unreachable
- **Import from Batch Files** – parse existing `.bat` files with `net use` commands to import drive definitions
- **Settings Dialog** – add, edit, and remove drive definitions with full input validation
- **Bilingual UI** – switch between English and German with a single click
- **Connection Log** – view, export, and clear error/warning log entries from within the app
- **Help Dialog** – built-in user guide covering all features

## Download

Download the latest release for your platform from the [**Releases**](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.

Each archive includes the self-contained application **and** an installer script that handles installation automatically:

| Platform | Archive | Installer |
|----------|---------|-----------|
| Windows (x64) | `NetworkDriveManager-windows-x64.zip` | `install.ps1` |
| macOS (Intel) | `NetworkDriveManager-macos-x64.tar.gz` | `install-macos.sh` |
| macOS (Apple Silicon) | `NetworkDriveManager-macos-arm64.tar.gz` | `install-macos.sh` |
| Linux (x64) | `NetworkDriveManager-linux-x64.tar.gz` | `install-linux.sh` |

## Installation

The application is published as a **self-contained** executable — no separate .NET runtime installation is required.

### Windows

1. Download `NetworkDriveManager-windows-x64.zip` from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.
2. Extract the archive.
3. Run the installer (creates Start Menu and Desktop shortcuts):
   ```powershell
   # Open PowerShell as Administrator and navigate to the extracted folder
   powershell -ExecutionPolicy Bypass -File install.ps1
   ```
4. Launch **Network Drive Manager** from the Start Menu or Desktop.

To uninstall:
```powershell
powershell -ExecutionPolicy Bypass -File "C:\Program Files\NetworkDriveManager\uninstall.ps1"
```

### macOS

1. Download `NetworkDriveManager-macos-arm64.tar.gz` (Apple Silicon) or `NetworkDriveManager-macos-x64.tar.gz` (Intel) from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.
2. Extract and install:
   ```bash
   tar -xzf NetworkDriveManager-macos-*.tar.gz
   cd NetworkDriveManager-macos-*
   sudo bash install-macos.sh
   ```
3. Launch from `/Applications/NetworkDriveManager/` or run `networkdrivemanager` in the terminal.

To uninstall:
```bash
sudo bash /Applications/NetworkDriveManager/uninstall-macos.sh
```

### Linux

1. Download `NetworkDriveManager-linux-x64.tar.gz` from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.
2. Extract and install (the installer automatically installs required system dependencies):
   ```bash
   tar -xzf NetworkDriveManager-linux-x64.tar.gz
   cd NetworkDriveManager-linux-x64
   sudo bash install-linux.sh
   ```
3. Launch from your application menu or run `networkdrivemanager` in the terminal.

The Linux installer automatically installs required dependencies (`cifs-utils` and GUI libraries) on Debian/Ubuntu, Fedora/RHEL, Arch, and openSUSE systems.

To uninstall:
```bash
sudo bash /opt/NetworkDriveManager/uninstall-linux.sh
```

## Requirements

The downloaded releases are **self-contained** and include the .NET runtime — no additional runtime installation is needed.

| Requirement | Details |
|---|---|
| **Windows** | Windows 10 or later (x64) |
| **macOS** | macOS 12 (Monterey) or later (Intel or Apple Silicon) |
| **Linux** | x64 distribution with `cifs-utils` installed (auto-installed by the installer script) |
| **.NET SDK** (build from source only) | 10.0 or later |

## Getting Started

### Build from Source

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
   dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

   # macOS (Apple Silicon)
   dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true

   # Linux
   dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
   ```

## Configuration

Drive definitions are stored in `drives_config.json` next to the executable. You can edit this file manually or use the **Settings** dialog inside the app.

```json
{
  "drives": [
    {
      "letter": "H",
      "server": "fileserver.example.local",
      "share": "Documents",
      "label": "Documents"
    }
  ]
}
```

| Field | Required | Description |
|---|---|---|
| `letter` | ✔ | Drive identifier (A–Z). Maps to a drive letter on Windows, or a mount point name on macOS/Linux |
| `server` | ✔ | Server hostname or IP address |
| `share` | ✔ | Share name on the server |
| `label` | ✔ | User-friendly display name |
| `hidden` | – | Hide the drive from the UI while keeping it in the config |

## Usage

1. **Enter credentials** – type your domain username (e.g. `example.local\john.doe`) and password. Click **Save Credentials** to persist them encrypted on disk.
2. **Manage drives** – use the **Connect All** / **Disconnect All** buttons for bulk operations, or use the **toggle button** on each drive row to connect or disconnect individual drives.
3. **Check server** – click the server indicator in a drive row to test SMB reachability.
4. **Refresh** – click **Refresh Status** to poll all drives immediately. The app also refreshes automatically every 30 seconds.
5. **Add or edit drives** – open **Settings** to add, edit, or remove drives. You can also import drives from a `.bat` file.
6. **View logs** – open the **Connection Log** from Settings to inspect entries, export them to CSV, or clear the log.
7. **Switch language** – click the language button to toggle between English and German.

## Project Structure

```
NetworkDriveManager/
├── .github/workflows/build.yml     # CI/CD pipeline (multi-platform build & release)
├── installers/                     # Platform-specific installer/uninstaller scripts
│   ├── install.ps1                 # Windows installer (PowerShell)
│   ├── uninstall.ps1               # Windows uninstaller (PowerShell)
│   ├── install-macos.sh            # macOS installer (Bash)
│   ├── uninstall-macos.sh          # macOS uninstaller (Bash)
│   ├── install-linux.sh            # Linux installer (Bash, auto-installs dependencies)
│   └── uninstall-linux.sh          # Linux uninstaller (Bash)
├── NetworkDriveManager.sln         # Solution file
├── README.md                       # Project documentation
├── CONTRIBUTING.md                 # Contribution guidelines
├── LICENSE                         # MIT License
└── NetworkDriveManager/            # Avalonia UI application project
    ├── Helpers/                    # MVVM base classes
    ├── Models/                     # Data models and translations
    ├── Services/                   # Business logic (config, credentials, drives, logging, server, platform detection)
    ├── ViewModels/                 # MVVM ViewModels
    └── Views/                      # Avalonia dialogs (Settings, Help)
```

## Documentation

For detailed documentation, visit the [**Project Site**](https://jakobeichberger.github.io/NetworkDriveManager/):

- [Installation Guide](https://jakobeichberger.github.io/NetworkDriveManager/installation) — platform-specific download and install instructions
- [Configuration](https://jakobeichberger.github.io/NetworkDriveManager/configuration) — drive definitions and settings reference
- [Architecture](https://jakobeichberger.github.io/NetworkDriveManager/architecture) — MVVM pattern, project structure, and CI/CD pipeline
- [Security](https://jakobeichberger.github.io/NetworkDriveManager/security) — credential encryption and data protection
- [Troubleshooting](https://jakobeichberger.github.io/NetworkDriveManager/troubleshooting) — common problems and solutions

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the [MIT License](LICENSE).
