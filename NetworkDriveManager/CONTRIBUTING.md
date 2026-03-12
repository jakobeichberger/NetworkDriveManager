# Contributing to Network Drive Manager

Thank you for your interest in contributing! This document provides guidelines to help you get started.

## Prerequisites

| Requirement     | Version          |
|-----------------|------------------|
| .NET SDK        | 10.0 or later    |
| OS              | Windows 10+, macOS 12+, or Linux (x64) |
| IDE (optional)  | Visual Studio 2022, VS Code with C# Dev Kit, or JetBrains Rider |

## Getting Started

1. **Fork** the repository and clone your fork:

   ```bash
   git clone https://github.com/<your-username>/NetworkDriveManager.git
   cd NetworkDriveManager
   ```

2. **Build** the project to make sure everything compiles:

   ```bash
   dotnet build NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj
   ```

3. **Run** the application:

   ```bash
   dotnet run --project NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj
   ```

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
├── CONTRIBUTING.md                  # This file
├── LICENSE                          # MIT License
└── NetworkDriveManager/             # Avalonia UI application project
    ├── Helpers/                     # MVVM base classes (ObservableObject, RelayCommand)
    ├── Models/                      # Data models and translations
    ├── Services/                    # Business logic (config, credentials, drives, logging, server, platform detection)
    ├── ViewModels/                  # MVVM ViewModels
    └── Views/                       # Avalonia dialogs (Settings, Help)
```

## Making Changes

1. Create a **feature branch** from `main`:

   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes, following the conventions below.

3. **Build** to verify your changes compile:

   ```bash
   dotnet build NetworkDriveManager/NetworkDriveManager/NetworkDriveManager.csproj
   ```

4. **Commit** with a clear, descriptive message:

   ```bash
   git commit -m "Add support for drive reconnection on wake"
   ```

5. **Push** your branch and open a Pull Request.

## Code Conventions

- **Language:** C# (latest version) with nullable reference types enabled
- **UI Framework:** Avalonia UI (cross-platform — not WPF)
- **Architecture:** MVVM (Model-View-ViewModel) pattern
- **XML Documentation:** All public classes, methods, and properties should have `/// <summary>` comments
- **Naming:** Follow standard [.NET naming conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
  - `PascalCase` for public members and types
  - `_camelCase` for private fields
  - `camelCase` for local variables and parameters
- **Code Style:** See `.editorconfig` for formatting rules (4-space indentation, UTF-8, etc.)
- **Translations:** When adding user-visible text, add entries to both `en` and `de` dictionaries in `Models/Translations.cs`
- **Platform Awareness:** Use `PlatformService` for any OS-specific logic; avoid hardcoding Windows-only paths or commands

## Reporting Issues

When reporting a bug, please include:

- Steps to reproduce the problem
- Expected vs. actual behavior
- Operating system and version (Windows, macOS, or Linux)
- .NET SDK version (`dotnet --version`)
- Any relevant log entries from `network_drive_manager.log`

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
