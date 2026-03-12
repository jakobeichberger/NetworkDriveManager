# Network Drive Manager

A Windows desktop application for managing multiple network drive mappings with encrypted credential storage and automatic monitoring.

## Features

- **Connect & Disconnect** individual drives or all drives at once
- **Encrypted Credentials** – username and password are stored using AES-GCM 256-bit encryption
- **Automatic Monitoring** – checks every 30 seconds whether connected drives are still mapped and alerts you if a drive unexpectedly disconnects
- **Server Reachability** – tests SMB (port 445) connectivity before connecting and warns you when a server becomes unreachable
- **Network Change Detection** – automatically re-checks drive status when network connectivity changes
- **Import from Batch Files** – parse existing `.bat` files with `net use` commands to import drive definitions
- **Settings Dialog** – add, edit, and remove drive definitions with full input validation
- **Bilingual UI** – switch between English and German with a single click
- **Connection Log** – view, export, and clear error/warning log entries from within the app
- **Help Dialog** – built-in user guide covering all features

## Requirements

| Requirement | Version |
|---|---|
| Windows | 10 / 11 |
| .NET Runtime | 8.0 or later |

## Getting Started

### Download

Download the latest release from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page, extract the archive, and run `NetworkDriveManager.exe`.

### Build from Source

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download).

2. Clone the repository:

   ```bash
   git clone https://github.com/jakobeichberger/NetworkDriveManager.git
   cd NetworkDriveManager
   ```

3. Build and run:

   ```bash
   dotnet run --project NetworkDriveManager
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
    },
    {
      "letter": "I",
      "server": "fileserver.example.local",
      "share": "Projects",
      "label": "Projects",
      "hidden": false
    }
  ]
}
```

| Field | Required | Description |
|---|---|---|
| `letter` | ✔ | Drive letter (A–Z) |
| `server` | ✔ | Server hostname or IP address |
| `share` | ✔ | Share name on the server |
| `label` | ✔ | User-friendly display name |
| `hidden` | – | Hide the drive from the UI while keeping it in the config |

Encrypted credentials (`secret.key`, `credentials.enc`) and log files (`network_drive_manager.log`) are also stored next to the executable.

## Usage

1. **Enter credentials** – type your domain username (e.g. `example.local\john.doe`) and password. Click **Save Credentials** to persist them encrypted on disk so they are pre-filled on the next launch.
2. **Manage drives** – use the **Connect All** / **Disconnect All** buttons for bulk operations, or toggle individual drives using the button in each row.
3. **Check server** – click the server indicator in a drive row to test SMB reachability.
4. **Refresh** – click **Refresh Status** to poll all drives immediately. The app also refreshes automatically every 30 seconds.
5. **Add or edit drives** – open the **Settings** dialog to add, edit, or remove drive definitions. You can also import drives from a `.bat` file.
6. **View logs** – open the **Connection Log** from the Settings dialog to inspect warning and error entries, export them to CSV, or clear the log.
7. **Switch language** – click the language button in the toolbar to toggle between English and German.

## Project Structure

```
NetworkDriveManager/
├── NetworkDriveManager.sln         # Visual Studio Solution
├── README.md                       # Project documentation
├── CONTRIBUTING.md                 # Contribution guidelines
├── LICENSE                         # MIT License
├── .editorconfig                   # Code style settings
└── NetworkDriveManager/            # WPF application project
    ├── Helpers/                    # MVVM base classes (ObservableObject, RelayCommand)
    ├── Models/                     # Data models and translations
    ├── Services/                   # Business logic (config, credentials, drives, logging, server)
    ├── ViewModels/                 # MVVM ViewModels
    └── Views/                      # WPF dialogs (Settings, Help)
```

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to set up the project, code conventions, and the pull request process.

## License

This project is licensed under the [MIT License](LICENSE).
