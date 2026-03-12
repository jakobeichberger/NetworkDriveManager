---
layout: default
title: Home
---

# Network Drive Manager

A cross-platform desktop application for managing SMB/CIFS network drive connections, built with **.NET 10** and **Avalonia UI 11.3**. Runs natively on **Windows**, **macOS**, and **Linux** with automatic platform detection.

## Features

- **Cross-platform** — automatically detects the operating system and uses the appropriate mount commands
- **Connect & Disconnect** individual drives or all drives at once using a toggle button
- **Encrypted Credentials** — username and password are stored using AES-GCM 256-bit encryption
- **Automatic Monitoring** — checks every 30 seconds whether connected drives are still mapped
- **Server Reachability** — tests SMB (port 445) connectivity before connecting
- **Import from Batch Files** — parse existing `.bat` files with `net use` commands to import drive definitions
- **Settings Dialog** — add, edit, and remove drive definitions with full input validation
- **Bilingual UI** — switch between English and German with a single click
- **Connection Log** — view, export, and clear error/warning log entries from within the app
- **Help Dialog** — built-in user guide covering all features

## Quick Links

| Page | Description |
|------|-------------|
| [Installation](installation) | Download and install on Windows, macOS, or Linux |
| [Configuration](configuration) | Set up and customise drive definitions |
| [Architecture](architecture) | MVVM pattern, project structure, and CI/CD pipeline |
| [Security](security) | Credential encryption and data protection details |
| [Troubleshooting](troubleshooting) | Common problems and their solutions |

## Download

Pre-built binaries for all platforms are available on the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.

| Platform | Archive |
|----------|---------|
| Windows (x64) | `NetworkDriveManager-windows-x64.zip` |
| macOS (Intel) | `NetworkDriveManager-macos-x64.tar.gz` |
| macOS (Apple Silicon) | `NetworkDriveManager-macos-arm64.tar.gz` |
| Linux (x64) | `NetworkDriveManager-linux-x64.tar.gz` |

## Getting Started

1. Download the archive for your platform from the [Releases](https://github.com/jakobeichberger/NetworkDriveManager/releases) page.
2. Extract the archive and run the included installer script.
3. Launch the application, enter your credentials, and start managing your network drives.

See the [Installation](installation) page for detailed platform-specific instructions.

## Contributing

Contributions are welcome! Please read the [Contributing Guidelines](https://github.com/jakobeichberger/NetworkDriveManager/blob/main/NetworkDriveManager/CONTRIBUTING.md) for details.

## License

This project is licensed under the [MIT License](https://github.com/jakobeichberger/NetworkDriveManager/blob/main/NetworkDriveManager/LICENSE).
