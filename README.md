# Network Drive Manager

A cross-platform desktop application for managing SMB/CIFS network drive connections, built with **.NET 10** and **Avalonia UI**. Runs natively on **Windows**, **macOS**, and **Linux** with automatic platform detection.

## Features

- **Cross-platform** — automatically detects Windows, macOS, or Linux and uses the appropriate mount commands
- **Connect & Disconnect** individual drives or all drives at once
- **Encrypted Credentials** – username and password are stored using AES-GCM 256-bit encryption
- **Automatic Monitoring** – checks every 30 seconds whether connected drives are still mapped and alerts you if a drive unexpectedly disconnects
- **Server Reachability** – tests SMB (port 445) connectivity before connecting and warns you when a server becomes unreachable
- **Import from Batch Files** – parse existing `.bat` files with `net use` commands to import drive definitions
- **Settings Dialog** – add, edit, and remove drive definitions with full input validation
- **Bilingual UI** – switch between English and German with a single click
- **Connection Log** – view, export, and clear error/warning log entries from within the app
- **Help Dialog** – built-in user guide covering all features

## Download

Download the latest release for your platform from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page:

| Platform | File |
|----------|------|
| Windows (x64) | `NetworkDriveManager-windows-x64.zip` |
| macOS (Intel) | `NetworkDriveManager-macos-x64.tar.gz` |
| macOS (Apple Silicon) | `NetworkDriveManager-macos-arm64.tar.gz` |
| Linux (x64) | `NetworkDriveManager-linux-x64.tar.gz` |

## Requirements

| Requirement | Version |
|---|---|
| OS | Windows 10+, macOS 12+, or Linux |
| .NET SDK (build only) | 10.0 or later |

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
2. **Manage drives** – use the **Connect All** / **Disconnect All** buttons for bulk operations, or toggle individual drives.
3. **Check server** – click the server indicator in a drive row to test SMB reachability.
4. **Refresh** – click **Refresh Status** to poll all drives immediately. The app also refreshes every 30 seconds.
5. **Add or edit drives** – open **Settings** to add, edit, or remove drives. You can also import drives from a `.bat` file.
6. **View logs** – open the **Connection Log** from Settings to inspect entries, export them to CSV, or clear the log.
7. **Switch language** – click the language button to toggle between English and German.

## Project Structure

```
NetworkDriveManager/
├── .github/workflows/build.yml     # CI/CD pipeline
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

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the [MIT License](LICENSE).
