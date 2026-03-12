# Network Drive Manager

A Windows desktop application for managing network drive connections, built with **.NET 8** and **WPF**.

## Features

- **Connect & disconnect** Windows network drives (SMB/CIFS shares) via a modern UI
- **Encrypted credential storage** using AES-GCM — username and password are stored locally and never leave the machine
- **Server reachability checks** — test whether the file server is online before connecting
- **Automatic monitoring** — background checks every 30 seconds detect unexpected disconnections or server mismatches
- **Bilingual UI** — switch between English and German at runtime
- **Settings dialog** — add, edit, or remove drives; mark drives as hidden; import from legacy batch scripts
- **Connection log** — view, clear, and export errors/warnings as CSV

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET SDK    | 8.0 or later |
| OS          | Windows 10 / 11 (WPF) |

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
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

Network drives are defined in `drives_config.json`:

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

| Property | Description |
|----------|-------------|
| `letter` | Windows drive letter (A–Z) |
| `server` | Hostname or IP address of the file server |
| `share`  | Name of the SMB share on the server |
| `label`  | Friendly display name shown in the UI |
| `hidden` | *(optional)* If `true`, the drive is kept in the config but hidden from the main window |

## Project Structure

```
NetworkDriveManager/
├── NetworkDriveManager.sln          # Solution file
├── README.md                        # This file
├── .gitignore                       # Git ignore rules
└── NetworkDriveManager/             # WPF application project
    ├── NetworkDriveManager.csproj
    ├── App.xaml / App.xaml.cs
    ├── MainWindow.xaml / MainWindow.xaml.cs
    ├── AssemblyInfo.cs
    ├── drives_config.json           # Example drive configuration
    ├── Helpers/
    │   ├── ObservableObject.cs      # INotifyPropertyChanged base
    │   └── RelayCommand.cs          # ICommand implementation
    ├── Models/
    │   ├── DriveConfig.cs           # Drive data model
    │   └── Translations.cs          # EN/DE UI strings
    ├── Services/
    │   ├── ConfigService.cs         # JSON config load/save
    │   ├── CredentialService.cs     # AES-GCM credential encryption
    │   ├── DriveService.cs          # net use wrapper
    │   ├── LogService.cs            # Rotating file logger
    │   └── ServerService.cs         # TCP port-445 reachability
    ├── ViewModels/
    │   └── MainViewModel.cs         # Application logic (MVVM)
    └── Views/
        ├── HelpDialog.xaml / .cs
        └── SettingsDialog.xaml / .cs
```

## License

This project is provided as-is. See the repository for license details.
