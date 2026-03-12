# Network Drive Manager

A cross-platform desktop application for managing SMB/CIFS network drive connections, built with **.NET 10** and **Avalonia UI**. Runs natively on **Windows**, **macOS**, and **Linux** with automatic platform detection.

## Features

- **Cross-platform** — automatically detects the operating system and uses the appropriate mount commands
  - Windows: `net use` drive letter mapping
  - macOS: `mount -t smbfs` to `/Volumes`
  - Linux: `mount -t cifs` to `/mnt`
- **Connect & disconnect** network drives (SMB/CIFS shares) via a modern GUI
- **Encrypted credential storage** using AES-GCM 256-bit — credentials are stored locally and never leave the machine
- **Server reachability checks** — test whether the file server is online (TCP port 445) before connecting
- **Automatic monitoring** — background checks every 30 seconds detect unexpected disconnections or server mismatches
- **Bilingual UI** — switch between English and German at runtime
- **Settings dialog** — add, edit, or remove drives; mark drives as hidden; import from legacy batch scripts
- **Connection log** — view, clear, and export errors/warnings as CSV
- **Rotating log files** — automatic cleanup when logs exceed 1 MB (keeps 3 backups)

## Download

Pre-built binaries for all platforms are available on the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page:

| Platform | Download |
|----------|----------|
| Windows (x64) | `NetworkDriveManager-windows-x64.zip` |
| macOS (Intel) | `NetworkDriveManager-macos-x64.tar.gz` |
| macOS (Apple Silicon) | `NetworkDriveManager-macos-arm64.tar.gz` |
| Linux (x64) | `NetworkDriveManager-linux-x64.tar.gz` |

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET SDK (for building) | 10.0 or later |
| OS | Windows 10+, macOS 12+, or Linux (with `cifs-utils` for SMB) |

> **Linux users:** Install `cifs-utils` to enable SMB mounting: `sudo apt install cifs-utils` (Debian/Ubuntu) or `sudo dnf install cifs-utils` (Fedora).

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/jakobeichberger/NetworkDriveManager.git
cd NetworkDriveManager
```

### 2. Build

```bash
dotnet build NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj
```

### 3. Run

```bash
dotnet run --project NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj
```

On first launch the application creates a `drives_config.json` with example network drives. Edit the file or use the **Settings** dialog to configure your own drives.

### 4. Publish a self-contained executable (optional)

```bash
# Windows
dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# macOS (Intel)
dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true

# macOS (Apple Silicon)
dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true

# Linux
dotnet publish NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
```

## Configuration

Network drives are defined in `drives_config.json` (created next to the executable on first run):

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

Each drive entry has the following properties:

| Property | Type     | Description |
|----------|----------|-------------|
| `letter` | `string` | Drive letter (A–Z). On Windows this maps to a drive letter; on macOS/Linux this becomes the mount point name (e.g. `H` → `/Volumes/H` or `/mnt/H`) |
| `server` | `string` | Hostname or IP address of the file server |
| `share`  | `string` | Name of the SMB share on the server |
| `label`  | `string` | Friendly display name shown in the UI |
| `hidden` | `bool`   | *(optional, default `false`)* If `true`, the drive is kept in the config but hidden from the main window |

## Platform-Specific Notes

### Windows
- Drives are mapped using the `net use` command with drive letters (e.g. `H:`)
- Username format: `DOMAIN\username` (e.g. `example.local\john.doe`)

### macOS
- Shares are mounted to `/Volumes/<letter>` using `mount -t smbfs`
- May require administrator privileges for mounting
- Username format: `DOMAIN\username`

### Linux
- Shares are mounted to `/mnt/<letter>` using `mount -t cifs`
- Requires `cifs-utils` package: `sudo apt install cifs-utils`
- May require root/sudo privileges for mounting
- Username format: `DOMAIN\username`

## Architecture

The application follows the **MVVM** (Model-View-ViewModel) pattern with a cross-platform UI layer:

```
┌──────────────────────────────────────────────────────────────┐
│  Views (Avalonia AXAML + code-behind)                        │
│  MainWindow · SettingsDialog · HelpDialog                    │
├──────────────────────────────────────────────────────────────┤
│  ViewModels                                                  │
│  MainViewModel · DriveRowViewModel · SettingsViewModel       │
├──────────────────────────────────────────────────────────────┤
│  Services                                                    │
│  ConfigService · CredentialService · DriveService             │
│  LogService · ServerService · PlatformService                │
├──────────────────────────────────────────────────────────────┤
│  Models                                                      │
│  DriveConfig · DrivesConfigFile · Translations                │
├──────────────────────────────────────────────────────────────┤
│  Helpers                                                     │
│  ObservableObject · RelayCommand                             │
└──────────────────────────────────────────────────────────────┘
```

| Layer          | Responsibility |
|----------------|----------------|
| **Views**      | Avalonia AXAML layouts and code-behind for dialogs; delegates all logic to ViewModels |
| **ViewModels** | Application state, commands, and UI-bound properties; communicates with Services |
| **Services**   | Stateless business logic — config I/O, credential encryption, platform-aware drive commands, logging, server pings, OS detection |
| **Models**     | Plain data classes (`DriveConfig`) and the bilingual translation dictionary |
| **Helpers**    | MVVM infrastructure — `ObservableObject` (property change notifications) and `RelayCommand` (command binding) |

## CI/CD Pipeline

The project includes an automated GitHub Actions pipeline (`.github/workflows/build.yml`) that:

1. **Builds** the application on every push and pull request to `main`
2. **Creates artifacts** for all platforms (Windows x64, macOS x64/arm64, Linux x64)
3. **Publishes releases** with downloadable binaries when a version tag is pushed (e.g. `v1.0.0`)

To create a release:
```bash
git tag v1.0.0
git push origin v1.0.0
```

## Security

- **Credential encryption:** Usernames and passwords are encrypted with **AES-256-GCM** before being written to `credentials.enc`. A random 256-bit key is stored in `secret.key`. Both files reside next to the executable and never leave the local machine.
- **No network transmission:** Credentials are only passed to the OS mount command; they are never sent over the network by the application itself.
- **Sensitive files are git-ignored:** `credentials.enc`, `secret.key`, and log files are listed in `.gitignore`.

## Runtime Files

The application creates several files next to the executable at runtime:

| File                           | Purpose |
|--------------------------------|---------|
| `drives_config.json`           | User-defined drive configuration |
| `credentials.enc`              | AES-GCM encrypted credentials |
| `secret.key`                   | 256-bit AES encryption key |
| `network_drive_manager.log`    | Current log file |
| `network_drive_manager.log.1–3`| Rotated log backups |

## Project Structure

```
NetworkDriveManager/
├── .github/workflows/
│   └── build.yml                    # CI/CD pipeline (build + release)
├── NetworkDriveManager.sln          # Visual Studio / dotnet Solution file
├── README.md                        # This file
├── CONTRIBUTING.md                  # Contribution guidelines
├── LICENSE                          # MIT License
├── .editorconfig                    # Code style settings
├── .gitignore                       # Git ignore rules
└── NetworkDriveManager/             # Avalonia application project
    ├── NetworkDriveManager.csproj   # Project file (.NET 10, Avalonia UI)
    ├── Program.cs                   # Application entry point
    ├── App.axaml / App.axaml.cs     # Avalonia application definition
    ├── MainWindow.axaml / .axaml.cs # Main window UI and code-behind
    ├── AssemblyInfo.cs              # Assembly metadata
    ├── drives_config.json           # Example drive configuration
    ├── Helpers/
    │   ├── ObservableObject.cs      # INotifyPropertyChanged base class
    │   └── RelayCommand.cs          # ICommand implementation for MVVM
    ├── Models/
    │   ├── DriveConfig.cs           # Drive configuration data model
    │   └── Translations.cs          # Bilingual UI strings (EN/DE)
    ├── Services/
    │   ├── ConfigService.cs         # JSON config file load/save
    │   ├── CredentialService.cs     # AES-GCM credential encryption
    │   ├── DriveService.cs          # Cross-platform drive mount/unmount
    │   ├── LogService.cs            # Rotating file-based logger
    │   ├── PlatformService.cs       # OS detection (Windows/macOS/Linux)
    │   └── ServerService.cs         # TCP port-445 reachability check
    ├── ViewModels/
    │   └── MainViewModel.cs         # Main application logic (MVVM ViewModel)
    └── Views/
        ├── HelpDialog.axaml / .cs   # Help / usage information dialog
        └── SettingsDialog.axaml / .cs # Drive management & log viewer dialog
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| **"The specified network password is not correct"** | Verify your username is in `DOMAIN\username` format and the password is correct |
| **Server shows as unreachable** | Ensure the file server is online, TCP port 445 is not blocked by a firewall, and the hostname resolves correctly |
| **Drive letters conflict** (Windows) | Each drive letter can only be used once; check for existing mappings with `net use` in a command prompt |
| **Permission denied when mounting** (Linux/macOS) | Run the application with `sudo` or configure appropriate mount permissions |
| **Application won't start** | Ensure .NET 10.0 Runtime is installed: `dotnet --list-runtimes` |
| **Credentials not saved** | Make sure the directory containing the executable is writable |
| **Log file growing too large** | Logs rotate automatically at 1 MB; you can also clear logs from the Settings dialog |

## Documentation

For detailed documentation, visit the [**Project Site**](https://jakobeichberger.github.io/NetworkDriveManager/):

- [Installation Guide](https://jakobeichberger.github.io/NetworkDriveManager/installation) — platform-specific download and install instructions
- [Configuration](https://jakobeichberger.github.io/NetworkDriveManager/configuration) — drive definitions and settings reference
- [Architecture](https://jakobeichberger.github.io/NetworkDriveManager/architecture) — MVVM pattern, project structure, and CI/CD pipeline
- [Security](https://jakobeichberger.github.io/NetworkDriveManager/security) — credential encryption and data protection
- [Troubleshooting](https://jakobeichberger.github.io/NetworkDriveManager/troubleshooting) — common problems and solutions

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
