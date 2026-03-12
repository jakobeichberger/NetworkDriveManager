---
layout: default
title: Configuration
---

# Configuration

Network drives are defined in `drives_config.json`, which is created next to the executable on first launch. You can edit this file manually or use the **Settings** dialog inside the app.

## Drive Configuration File

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
      "letter": "S",
      "server": "fileserver.example.local",
      "share": "SharedDrive",
      "label": "Shared Drive",
      "hidden": false
    }
  ]
}
```

## Drive Properties

| Property | Type     | Required | Description |
|----------|----------|----------|-------------|
| `letter` | `string` | ✔        | Drive identifier (A–Z). On Windows this maps to a drive letter (e.g. `H:`); on macOS it mounts to `/Volumes/H`; on Linux it mounts to `/mnt/H` |
| `server` | `string` | ✔        | Hostname or IP address of the file server |
| `share`  | `string` | ✔        | Name of the SMB share on the server |
| `label`  | `string` | ✔        | User-friendly display name shown in the application UI |
| `hidden` | `bool`   | –        | *(default: `false`)* If `true`, the drive is kept in the config but hidden from the main window |

## Credentials

Credentials are entered in the application UI and stored encrypted on disk. The username should be in **domain\username** format (e.g. `example.local\john.doe`).

- Click **Save Credentials** to persist credentials to `credentials.enc`.
- Credentials are encrypted using AES-256-GCM. See [Security](security) for details.

## Settings Dialog

The **Settings** dialog provides a graphical interface to manage drives:

- **Add** a new drive definition with full input validation
- **Edit** an existing drive's properties
- **Remove** a drive from the configuration
- **Hide/show** drives without removing them
- **Import from `.bat`** — parse legacy batch scripts containing `net use` commands

## Importing from Batch Files

If you have existing `.bat` files that use `net use` commands to map network drives, you can import them directly:

1. Open **Settings** from the main window.
2. Click **Import from .bat**.
3. Select your batch file.
4. The application parses `net use` commands and creates drive definitions automatically.

## Runtime Files

The application creates several files next to the executable at runtime:

| File                            | Purpose |
|---------------------------------|---------|
| `drives_config.json`            | User-defined drive configuration |
| `credentials.enc`               | AES-GCM encrypted credentials |
| `secret.key`                    | 256-bit AES encryption key |
| `network_drive_manager.log`     | Current log file |
| `network_drive_manager.log.1–3` | Rotated log backups (auto-cleanup at 1 MB) |
