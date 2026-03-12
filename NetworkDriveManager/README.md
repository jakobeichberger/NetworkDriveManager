# Network Drive Manager

A Windows desktop application for managing network drive connections, built with **.NET 8** and **WPF** (Windows Presentation Foundation).

## Features

- **Connect & disconnect** Windows network drives (SMB/CIFS shares) via a modern UI
- **Encrypted credential storage** using AES-GCM — username and password are stored locally and never leave the machine
- **Server reachability checks** — test whether the file server is online (TCP port 445) before connecting
- **Automatic monitoring** — background checks every 30 seconds detect unexpected disconnections or server mismatches
- **Bilingual UI** — switch between English and German at runtime
- **Settings dialog** — add, edit, or remove drives; mark drives as hidden; import from legacy batch scripts
- **Connection log** — view, clear, and export errors/warnings as CSV
- **Rotating log files** — automatic cleanup when logs exceed 1 MB (keeps 3 backups)

## Requirements

| Requirement | Version          |
|-------------|------------------|
| .NET SDK    | 8.0 or later     |
| OS          | Windows 10 / 11  |

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/jakobeichberger/NetworkDriveManager.git
cd NetworkDriveManager
```

### 2. Build

```bash
dotnet build
```

### 3. Run

```bash
dotnet run --project NetworkDriveManager
```

On first launch the application creates a `drives_config.json` with example network drives.
Edit the file or use the **Settings** dialog to configure your own drives.

### 4. Publish a single-file executable (optional)

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

The output will be in `NetworkDriveManager/bin/Release/net8.0-windows/win-x64/publish/`.

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
| `letter` | `string` | Windows drive letter (A–Z) |
| `server` | `string` | Hostname or IP address of the file server |
| `share`  | `string` | Name of the SMB share on the server |
| `label`  | `string` | Friendly display name shown in the UI |
| `hidden` | `bool`   | *(optional, default `false`)* If `true`, the drive is kept in the config but hidden from the main window |

## Architecture

The application follows the **MVVM** (Model-View-ViewModel) pattern:

```
┌──────────────────────────────────────────────────────────────┐
│  Views (XAML + code-behind)                                  │
│  MainWindow · SettingsDialog · HelpDialog                    │
├──────────────────────────────────────────────────────────────┤
│  ViewModels                                                  │
│  MainViewModel · DriveRowViewModel · SettingsViewModel       │
├──────────────────────────────────────────────────────────────┤
│  Services                                                    │
│  ConfigService · CredentialService · DriveService             │
│  LogService · ServerService                                  │
├──────────────────────────────────────────────────────────────┤
│  Models                                                      │
│  DriveConfig · DrivesConfigFile · Translations                │
├──────────────────────────────────────────────────────────────┤
│  Helpers                                                     │
│  ObservableObject · RelayCommand                             │
└──────────────────────────────────────────────────────────────┘
```

| Layer         | Responsibility |
|---------------|----------------|
| **Views**     | XAML layouts and code-behind for dialogs; delegates all logic to ViewModels |
| **ViewModels** | Application state, commands, and UI-bound properties; communicates with Services |
| **Services**  | Stateless business logic — config I/O, credential encryption, `net use` commands, logging, server pings |
| **Models**    | Plain data classes (`DriveConfig`) and the bilingual translation dictionary |
| **Helpers**   | MVVM infrastructure — `ObservableObject` (property change notifications) and `RelayCommand` (command binding) |

## Security

- **Credential encryption:** Usernames and passwords are encrypted with **AES-256-GCM** before being written to `credentials.enc`. A random 256-bit key is stored in `secret.key`. Both files reside next to the executable and never leave the local machine.
- **No network transmission:** Credentials are only passed to the Windows `net use` command; they are never sent over the network by the application itself.
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
├── NetworkDriveManager.sln          # Visual Studio Solution file
├── README.md                        # This file
├── CONTRIBUTING.md                  # Contribution guidelines
├── LICENSE                          # MIT License
├── .editorconfig                    # Code style settings
├── .gitignore                       # Git ignore rules
└── NetworkDriveManager/             # WPF application project
    ├── NetworkDriveManager.csproj   # Project file (.NET 8, WPF)
    ├── App.xaml / App.xaml.cs       # Application entry point
    ├── MainWindow.xaml / .xaml.cs   # Main window UI and code-behind
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
    │   ├── DriveService.cs          # Windows 'net use' command wrapper
    │   ├── LogService.cs            # Rotating file-based logger
    │   └── ServerService.cs         # TCP port-445 reachability check
    ├── ViewModels/
    │   └── MainViewModel.cs         # Main application logic (MVVM ViewModel)
    └── Views/
        ├── HelpDialog.xaml / .cs    # Help / usage information dialog
        └── SettingsDialog.xaml / .cs # Drive management & log viewer dialog
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| **"The specified network password is not correct"** | Verify your username is in `DOMAIN\username` format and the password is correct |
| **Server shows as unreachable** | Ensure the file server is online, TCP port 445 is not blocked by a firewall, and the hostname resolves correctly |
| **Drive letters conflict** | Each drive letter can only be used once; check for existing mappings with `net use` in a command prompt |
| **Application won't start** | Ensure .NET 8.0 Desktop Runtime is installed: `dotnet --list-runtimes` |
| **Credentials not saved** | Make sure the directory containing the executable is writable (not in `C:\Program Files`) |
| **Log file growing too large** | Logs rotate automatically at 1 MB; you can also clear logs from the Settings dialog |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
