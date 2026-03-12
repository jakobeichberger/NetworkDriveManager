# Contributing to Network Drive Manager

Thank you for your interest in contributing! This document provides guidelines to help you get started.

## Prerequisites

| Requirement     | Version          |
|-----------------|------------------|
| .NET SDK        | 8.0 or later     |
| OS              | Windows 10 / 11  |
| IDE (optional)  | Visual Studio 2022 or VS Code with C# Dev Kit |

## Getting Started

1. **Fork** the repository and clone your fork:

   ```bash
   git clone https://github.com/<your-username>/NetworkDriveManager.git
   cd NetworkDriveManager
   ```

2. **Build** the project to make sure everything compiles:

   ```bash
   dotnet build
   ```

3. **Run** the application:

   ```bash
   dotnet run --project NetworkDriveManager
   ```

## Project Structure

```
NetworkDriveManager/
├── NetworkDriveManager.sln         # Visual Studio Solution
├── README.md                       # Project documentation
├── CONTRIBUTING.md                 # This file
├── LICENSE                         # MIT License
├── .editorconfig                   # Code style settings
└── NetworkDriveManager/            # WPF application project
    ├── Helpers/                    # MVVM base classes (ObservableObject, RelayCommand)
    ├── Models/                     # Data models and translations
    ├── Services/                   # Business logic (config, credentials, drives, logging, server)
    ├── ViewModels/                 # MVVM ViewModels
    └── Views/                      # WPF dialogs (Settings, Help)
```

## Making Changes

1. Create a **feature branch** from `main`:

   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes, following the conventions below.

3. **Build** to verify your changes compile:

   ```bash
   dotnet build
   ```

4. **Commit** with a clear, descriptive message:

   ```bash
   git commit -m "Add support for drive reconnection on wake"
   ```

5. **Push** your branch and open a Pull Request.

## Code Conventions

- **Language:** C# 12 with nullable reference types enabled
- **Architecture:** MVVM (Model-View-ViewModel) pattern
- **XML Documentation:** All public classes, methods, and properties should have `/// <summary>` comments
- **Naming:** Follow standard [.NET naming conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
  - `PascalCase` for public members and types
  - `_camelCase` for private fields
  - `camelCase` for local variables and parameters
- **Code Style:** See `.editorconfig` for formatting rules (4-space indentation, UTF-8, etc.)
- **Translations:** When adding user-visible text, add entries to both `en` and `de` dictionaries in `Models/Translations.cs`

## Reporting Issues

When reporting a bug, please include:

- Steps to reproduce the problem
- Expected vs. actual behavior
- Windows version and .NET SDK version (`dotnet --version`)
- Any relevant log entries from `network_drive_manager.log`

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
