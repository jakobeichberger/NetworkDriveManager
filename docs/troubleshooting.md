---
layout: default
title: Troubleshooting
---

# Troubleshooting

This page covers common problems and their solutions. If you don't find your issue here, please [open an issue](https://github.com/jakobeichberger/NetworkDriveManager/issues/new) on GitHub.

## Common Issues

| Problem | Solution |
|---------|----------|
| **"The specified network password is not correct"** | Verify your username is in `DOMAIN\username` format (e.g. `example.local\john.doe`) and the password is correct. |
| **Server shows as unreachable** | Ensure the file server is online, TCP port 445 is not blocked by a firewall, and the hostname resolves correctly. |
| **Drive letters conflict** (Windows) | Each drive letter can only be used once. Check for existing mappings with `net use` in a command prompt. |
| **Permission denied when mounting** (Linux/macOS) | Run the application with `sudo` or configure appropriate mount permissions. |
| **Application won't start** | Ensure .NET 10.0 Runtime is installed (not needed for self-contained builds): `dotnet --list-runtimes`. |
| **Credentials not saved** | Make sure the directory containing the executable is writable by your user account. |
| **Log file growing too large** | Logs rotate automatically at 1 MB (keeps 3 backups). You can also clear logs from the Settings dialog. |

## Platform-Specific Issues

### Windows

| Problem | Solution |
|---------|----------|
| **`net use` returns "System error 53"** | The network path was not found. Verify the server hostname and share name. |
| **`net use` returns "System error 85"** | The drive letter is already in use. Disconnect the existing mapping first. |
| **Installer requires admin rights** | Run `install.ps1` from an elevated PowerShell prompt (Run as Administrator). |

### macOS

| Problem | Solution |
|---------|----------|
| **`mount_smbfs` fails with "Permission denied"** | Run the application with `sudo` or check that your user has mount permissions. |
| **Application is not signed** | macOS may block unsigned applications. Right-click the app and select **Open**, then confirm. |
| **Mount point already exists** | Another volume is mounted at the same path. Unmount it first with `umount /Volumes/<letter>`. |

### Linux

| Problem | Solution |
|---------|----------|
| **`mount -t cifs` not found** | Install `cifs-utils`: `sudo apt install cifs-utils` (Debian/Ubuntu), `sudo dnf install cifs-utils` (Fedora), or `sudo pacman -S cifs-utils` (Arch). |
| **GUI libraries missing** | The installer script automatically installs required GUI dependencies. If running manually, install `libice6`, `libsm6`, `libx11-6`, and related packages. |
| **Mount requires root** | Either run with `sudo` or add entries to `/etc/fstab` with the `user` option. |

## Checking Logs

The application writes logs to `network_drive_manager.log` in the same directory as the executable. You can also view logs from within the app:

1. Open **Settings**.
2. Switch to the **Connection Log** tab.
3. Review entries, **export to CSV**, or **clear** the log.

## Reporting a Bug

When opening an issue, please include:

- **Operating system** and version (e.g. Windows 11 23H2, macOS 14.3, Ubuntu 24.04)
- **Steps to reproduce** the problem
- **Expected vs. actual behaviour**
- `.NET SDK version` (if building from source): `dotnet --version`
- **Relevant log entries** from `network_drive_manager.log`
