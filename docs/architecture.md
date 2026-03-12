---
layout: default
title: Architecture
---

# Architecture

Network Drive Manager follows the **MVVM** (Model-View-ViewModel) pattern with a cross-platform UI layer built on Avalonia UI.

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Language** | C# (latest) |
| **Runtime** | .NET 10 |
| **UI Framework** | Avalonia UI 11.3 |
| **Pattern** | MVVM (Model-View-ViewModel) |
| **Platforms** | Windows, macOS, Linux |

## Layer Diagram

```
┌──────────────────────────────────────────────────────────────┐
│  Views (Avalonia AXAML + code-behind)                        │
│  MainWindow · SettingsDialog · HelpDialog                    │
├──────────────────────────────────────────────────────────────┤
│  ViewModels                                                  │
│  MainViewModel                                               │
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

## Layer Responsibilities

| Layer          | Responsibility |
|----------------|----------------|
| **Views**      | Avalonia AXAML layouts and code-behind for dialogs; delegates all logic to ViewModels |
| **ViewModels** | Application state, commands, and UI-bound properties; communicates with Services |
| **Services**   | Stateless business logic — config I/O, credential encryption, platform-aware drive commands, logging, server pings, OS detection |
| **Models**     | Plain data classes (`DriveConfig`) and the bilingual translation dictionary |
| **Helpers**    | MVVM infrastructure — `ObservableObject` (property change notifications) and `RelayCommand` (command binding) |

## Key Services

### PlatformService

Detects the current operating system at runtime using `RuntimeInformation.IsOSPlatform()` and exposes platform-specific paths and flags.

### DriveService

Handles mount and unmount operations with OS-specific commands:

- **Windows** — `net use` with drive letters (e.g. `H:`)
- **macOS** — `mount_smbfs` to `/Volumes/<letter>` with credentials passed via the `PASSWD` environment variable
- **Linux** — `mount -t cifs` to `/mnt/<letter>` with a temporary credentials file (`chmod 600`)

### CredentialService

Encrypts and decrypts credentials using AES-256-GCM. On Unix systems, file permissions for `secret.key` and `credentials.enc` are set to `0600`.

### ConfigService

Reads and writes the `drives_config.json` file using `System.Text.Json`.

### LogService

Provides rotating file-based logging. Log files are automatically rotated when they exceed 1 MB, keeping up to 3 backups.

### ServerService

Tests SMB server reachability by attempting a TCP connection to port 445.

## Project Structure

```
NetworkDriveManager/
├── .github/workflows/
│   └── build.yml                    # CI/CD pipeline (build + release)
├── installers/                      # Platform-specific installer/uninstaller scripts
│   ├── install.ps1 / uninstall.ps1  # Windows (PowerShell)
│   ├── install-macos.sh / uninstall-macos.sh  # macOS (Bash)
│   └── install-linux.sh / uninstall-linux.sh  # Linux (Bash)
├── docs/                            # Project documentation site (GitHub Pages)
├── NetworkDriveManager.sln          # Visual Studio Solution
├── README.md                        # Project documentation
├── CONTRIBUTING.md                  # Contribution guidelines
├── LICENSE                          # MIT License
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

## CI/CD Pipeline

The project includes an automated GitHub Actions pipeline (`.github/workflows/build.yml`) that:

1. **Builds** the application on every push and pull request to `main`
2. **Creates artifacts** for all four platforms (Windows x64, macOS x64, macOS arm64, Linux x64)
3. **Publishes releases** with downloadable binaries when a version tag is pushed (e.g. `v1.0.0`)

Each release archive includes the application binary and a platform-specific installer script.

### Creating a Release

```bash
git tag v1.0.0
git push origin v1.0.0
```

This triggers the CI pipeline, which builds all platform variants and creates a GitHub Release with the archives attached.
