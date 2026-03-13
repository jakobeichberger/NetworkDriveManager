using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using NetworkDriveManager.Helpers;
using NetworkDriveManager.Models;
using NetworkDriveManager.Services;

namespace NetworkDriveManager.ViewModels;

/// <summary>
/// Represents a single drive row in the main window's drive list.
/// </summary>
public class DriveRowViewModel : ObservableObject
{
    /// <summary>The underlying drive configuration.</summary>
    public DriveConfig Config { get; }

    /// <summary>Drive letter from the configuration.</summary>
    public string Letter => Config.Letter;
    /// <summary>Formatted display string showing the label and UNC path.</summary>
    public string DisplayPath => $"{Config.Label}  ({Config.UncPath})";

    private bool _isConnected;
    /// <summary>Whether the drive is currently connected.</summary>
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(ToggleButtonText));
                OnPropertyChanged(nameof(ToggleButtonColor));
                OnPropertyChanged(nameof(ServerCheckText));
                OnPropertyChanged(nameof(ServerCheckColor));
            }
        }
    }

    private bool? _serverReachable;
    /// <summary>Server reachability state: null = not checked, true = reachable, false = unreachable.</summary>
    public bool? ServerReachable
    {
        get => _serverReachable;
        set
        {
            if (SetProperty(ref _serverReachable, value))
            {
                OnPropertyChanged(nameof(ServerCheckText));
                OnPropertyChanged(nameof(ServerCheckColor));
            }
        }
    }

    private bool? _drivePermission;
    /// <summary>Drive permission state: null = not checked, true = read/write, false = read-only.</summary>
    public bool? DrivePermission
    {
        get => _drivePermission;
        set
        {
            if (SetProperty(ref _drivePermission, value))
            {
                OnPropertyChanged(nameof(ServerCheckText));
                OnPropertyChanged(nameof(ServerCheckColor));
            }
        }
    }

    private bool _isCheckingServer;
    /// <summary>Whether a server reachability check is in progress.</summary>
    public bool IsCheckingServer
    {
        get => _isCheckingServer;
        set => SetProperty(ref _isCheckingServer, value);
    }

    private string _lang = "de";
    /// <summary>Current UI language code (de or en).</summary>
    public string Lang
    {
        get => _lang;
        set
        {
            if (SetProperty(ref _lang, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(ToggleButtonText));
                OnPropertyChanged(nameof(ServerCheckText));
            }
        }
    }

    /// <summary>Localized connection status text.</summary>
    public string StatusText => IsConnected
        ? Translations.Get(Lang, "connected")
        : Translations.Get(Lang, "disconnected");

    /// <summary>Color code for the connection status indicator.</summary>
    public string StatusColor => IsConnected ? "#2e7d32" : "#b71c1c";

    /// <summary>Localized connect/disconnect button text.</summary>
    public string ToggleButtonText => IsConnected
        ? Translations.Get(Lang, "disconnect")
        : Translations.Get(Lang, "connect");

    /// <summary>Color code for the toggle button.</summary>
    public string ToggleButtonColor => IsConnected ? "#2e7d32" : "#b71c1c";

    /// <summary>Localized server check / permission status text.</summary>
    public string ServerCheckText
    {
        get
        {
            if (IsCheckingServer) return Translations.Get(Lang, "working");

            if (IsConnected)
            {
                // Drive is connected: show permission info
                return DrivePermission switch
                {
                    true => Translations.Get(Lang, "drive_readwrite"),
                    false => Translations.Get(Lang, "drive_readonly"),
                    null => Translations.Get(Lang, "server_check_btn"),
                };
            }

            // Drive is not connected: show server reachability
            return ServerReachable switch
            {
                true => Translations.Get(Lang, "server_reachable"),
                false => Translations.Get(Lang, "server_unreachable_short"),
                null => Translations.Get(Lang, "server_check_btn"),
            };
        }
    }

    /// <summary>Color code for the server check / permission status indicator.</summary>
    public string ServerCheckColor
    {
        get
        {
            if (IsCheckingServer) return "#78909c";

            if (IsConnected)
            {
                // Drive is connected: color based on permission
                return DrivePermission switch
                {
                    true => "#2e7d32",    // green for read/write
                    false => "#e65100",    // orange for read-only
                    null => "#78909c",     // gray for unchecked
                };
            }

            // Drive is not connected: color based on reachability
            return ServerReachable switch
            {
                true => "#2e7d32",
                false => "#b71c1c",
                null => "#78909c",
            };
        }
    }

    /// <summary>Initializes a new drive row view model from the given configuration.</summary>
    public DriveRowViewModel(DriveConfig config) => Config = config;

    /// <summary>
    /// Forces a re-evaluation of all toggle-related bindings to sync the ToggleButton visual state.
    /// Call this after an operation completes (success or failure) to ensure the UI reflects the actual state.
    /// </summary>
    public void RefreshToggleState()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(ToggleButtonText));
        OnPropertyChanged(nameof(ToggleButtonColor));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(ServerCheckText));
        OnPropertyChanged(nameof(ServerCheckColor));
    }
}

/// <summary>
/// Represents a warning in the warnings panel.
/// </summary>
public class WarningItem : ObservableObject
{
    /// <summary>Unique identifier for the warning.</summary>
    public string Key { get; set; } = string.Empty;

    private string _message = string.Empty;
    /// <summary>Warning message text displayed to the user.</summary>
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}

/// <summary>
/// Main ViewModel for the NetworkDriveApp window.
/// </summary>
public class MainViewModel : ObservableObject
{
    /// <summary>Avalonia dispatcher for UI thread marshalling.</summary>
    private readonly Dispatcher _dispatcher;
    /// <summary>Timer for periodic drive monitoring.</summary>
    private System.Threading.Timer? _monitorTimer;
    /// <summary>Flag indicating whether an async operation is in progress.</summary>
    private bool _busy;

    /// <summary>How often (seconds) the background monitor checks connected drives.</summary>
    private const int MonitorIntervalSeconds = 30;

    private string _lang = "de";
    /// <summary>Current UI language code (de or en).</summary>
    public string Lang
    {
        get => _lang;
        set
        {
            if (SetProperty(ref _lang, value))
            {
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(AppSubtitle));
                OnPropertyChanged(nameof(CredentialsHeader));
                OnPropertyChanged(nameof(UsernameLabel));
                OnPropertyChanged(nameof(PasswordLabel));
                OnPropertyChanged(nameof(SaveCredentialsText));
                OnPropertyChanged(nameof(NetworkDrivesHeader));
                OnPropertyChanged(nameof(DriveHeader));
                OnPropertyChanged(nameof(PathHeader));
                OnPropertyChanged(nameof(StatusHeader));
                OnPropertyChanged(nameof(ServerCheckHeader));
                OnPropertyChanged(nameof(ActionHeader));
                OnPropertyChanged(nameof(ConnectAllText));
                OnPropertyChanged(nameof(DisconnectAllText));
                OnPropertyChanged(nameof(RefreshStatusText));
                OnPropertyChanged(nameof(LangButtonText));
                OnPropertyChanged(nameof(SettingsText));
                OnPropertyChanged(nameof(HelpText));
                OnPropertyChanged(nameof(WarningsHeader));
                OnPropertyChanged(nameof(UsernameHintText));
                OnPropertyChanged(nameof(PasswordHintText));

                // Update all drive rows
                foreach (var row in DriveRows)
                    row.Lang = value;
            }
        }
    }

    /// <summary>Shorthand for translation lookup using the current language.</summary>
    public string T(string key) => Translations.Get(Lang, key);

    /// <summary>Localized UI text properties bound to the view.</summary>
    public string WindowTitle => T("window_title");
    public string AppSubtitle => T("app_subtitle");
    public string CredentialsHeader => T("credentials");
    public string UsernameLabel => T("username");
    public string PasswordLabel => T("password");
    public string SaveCredentialsText => T("save_credentials");
    public string NetworkDrivesHeader => T("network_drives");
    public string DriveHeader => T("drive");
    public string PathHeader => T("path");
    public string StatusHeader => T("status");
    public string ServerCheckHeader => T("server_check");
    public string ActionHeader => T("action");
    public string ConnectAllText => T("connect_all");
    public string DisconnectAllText => T("disconnect_all");
    public string RefreshStatusText => T("refresh_status");
    public string LangButtonText => T("lang_button");
    public string SettingsText => T("settings");
    public string HelpText => T("help");
    public string WarningsHeader => T("warnings");

    private string _username = string.Empty;
    /// <summary>Network credential username.</summary>
    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                OnPropertyChanged(nameof(UsernameHintText));
                OnPropertyChanged(nameof(UsernameHintColor));
                OnPropertyChanged(nameof(UsernameIndicatorColor));
            }
        }
    }

    private string _password = string.Empty;
    /// <summary>Network credential password.</summary>
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                OnPropertyChanged(nameof(PasswordHintText));
                OnPropertyChanged(nameof(PasswordHintColor));
                OnPropertyChanged(nameof(PasswordIndicatorColor));
            }
        }
    }

    /// <summary>Validation hint text for the username field.</summary>
    public string UsernameHintText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Username)) return T("username_hint");
            return Username.Contains('\\') ? T("username_hint_valid") : T("username_hint_invalid");
        }
    }

    /// <summary>Color for the username hint text.</summary>
    public string UsernameHintColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Username)) return "#6b7280";
            return Username.Contains('\\') ? "#2e7d32" : "#b71c1c";
        }
    }

    /// <summary>Color for the username validation indicator.</summary>
    public string UsernameIndicatorColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Username)) return "#6b7280";
            return Username.Contains('\\') ? "#2e7d32" : "#b71c1c";
        }
    }

    /// <summary>Validation hint text for the password field.</summary>
    public string PasswordHintText
    {
        get
        {
            if (string.IsNullOrEmpty(Password)) return T("password_hint");
            return T("password_hint_valid");
        }
    }

    /// <summary>Color for the password hint text.</summary>
    public string PasswordHintColor =>
        string.IsNullOrEmpty(Password) ? "#6b7280" : "#2e7d32";

    /// <summary>Color for the password validation indicator.</summary>
    public string PasswordIndicatorColor =>
        string.IsNullOrEmpty(Password) ? "#6b7280" : "#2e7d32";

    /// <summary>Observable collection of drive row view models.</summary>
    public ObservableCollection<DriveRowViewModel> DriveRows { get; } = new();

    /// <summary>Observable collection of warning items.</summary>
    public ObservableCollection<WarningItem> Warnings { get; } = new();
    /// <summary>Whether any warnings exist.</summary>
    public bool HasWarnings => Warnings.Count > 0;

    private string _statusMessage = string.Empty;
    /// <summary>Status bar message text.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private string _statusColor = "#1565c0";
    /// <summary>Status bar color.</summary>
    public string StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    /// <summary>Whether an async operation is currently running.</summary>
    public bool IsBusy
    {
        get => _busy;
        set
        {
            if (SetProperty(ref _busy, value))
                OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    /// <summary>Inverse of <see cref="IsBusy"/> for data binding.</summary>
    public bool IsNotBusy => !_busy;

    /// <summary>Command to toggle UI language between German and English.</summary>
    public ICommand ToggleLanguageCommand { get; }
    /// <summary>Command to save credentials to encrypted storage.</summary>
    public ICommand SaveCredentialsCommand { get; }
    /// <summary>Command to connect all disconnected drives.</summary>
    public ICommand ConnectAllCommand { get; }
    /// <summary>Command to disconnect all connected drives.</summary>
    public ICommand DisconnectAllCommand { get; }
    /// <summary>Command to refresh the connection status of all drives.</summary>
    public ICommand RefreshStatusCommand { get; }
    /// <summary>Command to toggle connection state of a single drive.</summary>
    public ICommand ToggleDriveCommand { get; }
    /// <summary>Command to check server reachability for a drive.</summary>
    public ICommand CheckServerCommand { get; }
    /// <summary>Command to dismiss a single warning by key.</summary>
    public ICommand DismissWarningCommand { get; }
    /// <summary>Command to open the settings dialog.</summary>
    public ICommand OpenSettingsCommand { get; }
    /// <summary>Command to open the help dialog.</summary>
    public ICommand OpenHelpCommand { get; }

    /// <summary>Raised when the user requests the settings dialog.</summary>
    public event Action? SettingsRequested;
    /// <summary>Raised when the user requests the help dialog.</summary>
    public event Action? HelpRequested;
    /// <summary>Raised when a message box should be shown (title, message, type).</summary>
    public event Action<string, string, string>? MessageBoxRequested; // title, msg, type

    /// <summary>Initializes the view model, loads drives and credentials, and starts background monitoring.</summary>
    public MainViewModel()
    {
        _dispatcher = Dispatcher.UIThread;

        ToggleLanguageCommand = new RelayCommand(ToggleLanguage);
        SaveCredentialsCommand = new RelayCommand(DoSaveCredentials);
        ConnectAllCommand = new RelayCommand(OnConnectAll, () => IsNotBusy);
        DisconnectAllCommand = new RelayCommand(OnDisconnectAll, () => IsNotBusy);
        RefreshStatusCommand = new RelayCommand(OnRefreshStatus, () => IsNotBusy);
        ToggleDriveCommand = new RelayCommand(p => OnToggleDrive(p as DriveRowViewModel));
        CheckServerCommand = new RelayCommand(p => OnCheckServer(p as DriveRowViewModel));
        DismissWarningCommand = new RelayCommand(p => DismissWarning(p as string));
        OpenSettingsCommand = new RelayCommand(() => SettingsRequested?.Invoke());
        OpenHelpCommand = new RelayCommand(() => HelpRequested?.Invoke());

        Warnings.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasWarnings));

        LoadDrives();
        LoadSavedCredentials();

        // Start background monitoring
        _monitorTimer = new System.Threading.Timer(_ => MonitorDrives(), null,
            TimeSpan.FromSeconds(MonitorIntervalSeconds),
            TimeSpan.FromSeconds(MonitorIntervalSeconds));

        // Kick off initial status checks
        Task.Run(RefreshAllStatusesAsync);
        Task.Run(RefreshAllServerPingsAsync);

        LogService.Info("Application started");
    }

    /// <summary>Switches the UI language between German and English.</summary>
    private void ToggleLanguage()
    {
        Lang = Lang == "de" ? "en" : "de";
    }

    /// <summary>Loads encrypted credentials from disk and pre-fills the fields.</summary>
    private void LoadSavedCredentials()
    {
        var (user, pass) = CredentialService.LoadCredentials();
        if (user != null)
        {
            Username = user;
            Password = pass ?? string.Empty;
            LogService.Info($"Pre-filled saved credentials for user '{user}'");
        }
    }

    /// <summary>Encrypts and saves the current credentials to disk.</summary>
    private void DoSaveCredentials()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
        {
            MessageBoxRequested?.Invoke(T("missing_creds_title"), T("missing_creds_msg"), "warning");
            return;
        }
        try
        {
            CredentialService.SaveCredentials(Username.Trim(), Password);
            MessageBoxRequested?.Invoke(T("credentials_saved"), T("credentials_saved_msg"), "info");
        }
        catch (Exception ex)
        {
            LogService.Error($"Failed to save credentials: {ex.Message}");
            MessageBoxRequested?.Invoke(T("error"), string.Format(T("credentials_save_failed"), ex.Message), "error");
        }
    }

    /// <summary>Loads drive configurations and populates <see cref="DriveRows"/>.</summary>
    public void LoadDrives()
    {
        DriveRows.Clear();
        var drives = ConfigService.LoadConfig();
        foreach (var d in drives.Where(d => !d.Hidden))
        {
            DriveRows.Add(new DriveRowViewModel(d) { Lang = Lang });
        }
    }

    /// <summary>Reloads drive configurations and refreshes all statuses.</summary>
    public void ReloadDrives()
    {
        _dispatcher.Invoke(() =>
        {
            ClearAllWarnings();
            LoadDrives();
        });
        Task.Run(RefreshAllStatusesAsync);
        Task.Run(RefreshAllServerPingsAsync);
    }

    /// <summary>Triggers an async status refresh for all drives.</summary>
    private void OnRefreshStatus()
    {
        if (IsBusy) return;
        IsBusy = true;
        Task.Run(async () =>
        {
            await RefreshAllStatusesAsync();
            _dispatcher.Invoke(() => IsBusy = false);
        });
    }

    /// <summary>Checks connection status of all drives in parallel.</summary>
    private async Task RefreshAllStatusesAsync()
    {
        var tasks = DriveRows.Select(row => Task.Run(() =>
        {
            var (connected, remote) = DriveService.GetDriveInfo(row.Letter);
            bool? permission = connected ? DriveService.HasWriteAccess(row.Letter) : null;
            _dispatcher.Invoke(() =>
            {
                row.IsConnected = connected;
                row.DrivePermission = permission;
                if (!connected) ClearWarning($"mismatch_{row.Letter}");

                if (connected)
                {
                    var expectedUnc = row.Config.UncPath.ToLowerInvariant();
                    if (remote != null && remote.ToLowerInvariant() != expectedUnc)
                    {
                        AddWarning($"mismatch_{row.Letter}",
                            string.Format(T("server_mismatch_status"),
                                row.Letter, remote, row.Config.Server, row.Config.Share));
                    }
                }
            });
        })).ToArray();

        await Task.WhenAll(tasks);
    }

    /// <summary>Connects or disconnects a single drive.</summary>
    private void OnToggleDrive(DriveRowViewModel? row)
    {
        if (row == null || IsBusy) return;

        string? username = null, password = null;
        if (!row.IsConnected)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
            {
                MessageBoxRequested?.Invoke(T("missing_creds_title"), T("missing_creds_msg"), "warning");
                return;
            }
            username = Username.Trim();
            password = Password;
        }

        IsBusy = true;
        Task.Run(() =>
        {
            try
            {
                if (row.IsConnected)
                {
                    var (ok, msg) = DriveService.DisconnectDrive(row.Letter);
                    _dispatcher.Invoke(() =>
                    {
                        if (ok)
                        {
                            row.IsConnected = false;
                            row.DrivePermission = null;
                            ClearWarning($"mismatch_{row.Letter}");
                        }
                        else
                            MessageBoxRequested?.Invoke(T("error"),
                                string.Format(T("disconnect_failed"), row.Letter, msg), "error");
                    });
                }
                else
                {
                    if (!ServerService.IsServerReachable(row.Config.Server))
                    {
                        _dispatcher.Invoke(() =>
                            MessageBoxRequested?.Invoke(T("server_unreachable_title"),
                                string.Format(T("server_unreachable_msg"), row.Config.Server), "warning"));
                        return;
                    }

                    var (ok, msg) = DriveService.ConnectDrive(
                        row.Letter, row.Config.Server, row.Config.Share, username!, password!);
                    if (ok)
                    {
                        var permission = DriveService.HasWriteAccess(row.Letter);
                        _dispatcher.Invoke(() =>
                        {
                            row.IsConnected = true;
                            row.DrivePermission = permission;
                        });
                    }
                    else
                    {
                        _dispatcher.Invoke(() =>
                            MessageBoxRequested?.Invoke(T("error"),
                                string.Format(T("connect_failed"), row.Letter, msg), "error"));
                    }
                }
            }
            finally
            {
                _dispatcher.Invoke(() =>
                {
                    row.RefreshToggleState();
                    IsBusy = false;
                });
            }
        });
    }

    /// <summary>Connects all disconnected drives.</summary>
    private void OnConnectAll()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(Password))
        {
            MessageBoxRequested?.Invoke(T("missing_creds_title"), T("missing_creds_msg"), "warning");
            return;
        }

        var username = Username.Trim();
        var password = Password;

        IsBusy = true;
        Task.Run(() =>
        {
            try
            {
                // Check server reachability first
                var servers = DriveRows
                    .Where(r => !r.IsConnected)
                    .Select(r => r.Config.Server)
                    .Distinct()
                    .ToList();

                var unreachable = servers.Where(s => !ServerService.IsServerReachable(s)).ToList();
                if (unreachable.Count > 0)
                {
                    _dispatcher.Invoke(() =>
                        MessageBoxRequested?.Invoke(T("server_unreachable_title"),
                            string.Format(T("server_unreachable_msg"), string.Join(", ", unreachable)), "warning"));
                    return;
                }

                var errors = new List<string>();
                foreach (var row in DriveRows.Where(r => !r.IsConnected))
                {
                    var (ok, msg) = DriveService.ConnectDrive(
                        row.Letter, row.Config.Server, row.Config.Share, username, password);
                    if (ok)
                    {
                        var permission = DriveService.HasWriteAccess(row.Letter);
                        _dispatcher.Invoke(() =>
                        {
                            row.IsConnected = true;
                            row.DrivePermission = permission;
                        });
                    }
                    else
                        errors.Add($"{row.Letter}: {msg}");
                }

                if (errors.Count > 0)
                {
                    _dispatcher.Invoke(() =>
                        MessageBoxRequested?.Invoke(T("errors"),
                            string.Format(T("connect_errors"), string.Join("\n", errors)), "error"));
                }
            }
            finally
            {
                _dispatcher.Invoke(() => IsBusy = false);
            }
        });
    }

    /// <summary>Disconnects all connected drives.</summary>
    private void OnDisconnectAll()
    {
        IsBusy = true;
        Task.Run(() =>
        {
            try
            {
                var errors = new List<string>();
                foreach (var row in DriveRows.Where(r => r.IsConnected))
                {
                    var (ok, msg) = DriveService.DisconnectDrive(row.Letter);
                    if (ok)
                        _dispatcher.Invoke(() =>
                        {
                            row.IsConnected = false;
                            row.DrivePermission = null;
                            ClearWarning($"mismatch_{row.Letter}");
                        });
                    else
                        errors.Add($"{row.Letter}: {msg}");
                }

                if (errors.Count > 0)
                {
                    _dispatcher.Invoke(() =>
                        MessageBoxRequested?.Invoke(T("errors"),
                            string.Format(T("disconnect_errors"), string.Join("\n", errors)), "error"));
                }
            }
            finally
            {
                _dispatcher.Invoke(() => IsBusy = false);
            }
        });
    }

    /// <summary>Checks server reachability or drive permissions depending on connection state.</summary>
    private void OnCheckServer(DriveRowViewModel? row)
    {
        if (row == null) return;
        var server = row.Config.Server;

        // Set all drives on same server to "checking"
        foreach (var r in DriveRows.Where(r => r.Config.Server == server))
            r.IsCheckingServer = true;

        Task.Run(() =>
        {
            // For connected drives: check permissions
            // For disconnected drives: check server reachability
            var connectedOnServer = DriveRows.Where(r => r.Config.Server == server && r.IsConnected).ToList();
            var disconnectedOnServer = DriveRows.Where(r => r.Config.Server == server && !r.IsConnected).ToList();

            // Check permissions for connected drives
            foreach (var r in connectedOnServer)
            {
                var permission = DriveService.HasWriteAccess(r.Letter);
                _dispatcher.Invoke(() =>
                {
                    r.DrivePermission = permission;
                    r.IsCheckingServer = false;
                });
            }

            // Check reachability for disconnected drives
            if (disconnectedOnServer.Count > 0)
            {
                var reachable = ServerService.IsServerReachable(server);
                _dispatcher.Invoke(() =>
                {
                    foreach (var r in disconnectedOnServer)
                    {
                        r.ServerReachable = reachable;
                        r.IsCheckingServer = false;
                    }
                });
            }
        });
    }

    /// <summary>Pings all unique servers in parallel, checks permissions for connected drives.</summary>
    private async Task RefreshAllServerPingsAsync()
    {
        // Group by server to avoid duplicate reachability checks
        var serverGroups = DriveRows.GroupBy(r => r.Config.Server).ToList();

        _dispatcher.Invoke(() =>
        {
            foreach (var row in DriveRows)
                row.IsCheckingServer = true;
        });

        var tasks = serverGroups.Select(g => Task.Run(() =>
        {
            var connectedRows = g.Where(r => r.IsConnected).ToList();
            var disconnectedRows = g.Where(r => !r.IsConnected).ToList();

            // Check permissions for connected drives
            foreach (var row in connectedRows)
            {
                var permission = DriveService.HasWriteAccess(row.Letter);
                _dispatcher.Invoke(() =>
                {
                    row.DrivePermission = permission;
                    row.IsCheckingServer = false;
                });
            }

            // Check reachability for disconnected drives
            if (disconnectedRows.Count > 0)
            {
                var reachable = ServerService.IsServerReachable(g.Key);
                _dispatcher.Invoke(() =>
                {
                    foreach (var row in disconnectedRows)
                    {
                        row.ServerReachable = reachable;
                        row.IsCheckingServer = false;
                    }
                });
            }
        })).ToArray();

        await Task.WhenAll(tasks);
    }

    /// <summary>Background task that checks for unexpected drive disconnections.</summary>
    private void MonitorDrives()
    {
        var connectedRows = DriveRows.Where(r => r.IsConnected).ToList();
        if (connectedRows.Count == 0) return;

        var tasks = connectedRows.Select(row => Task.Run(() =>
        {
            var (stillConnected, remote) = DriveService.GetDriveInfo(row.Letter);
            if (!stillConnected)
            {
                _dispatcher.Invoke(() =>
                {
                    if (!row.IsConnected) return; // already handled
                    row.IsConnected = false;
                    row.DrivePermission = null;
                    var label = row.Config.Label;
                    LogService.Warning($"Drive {row.Letter}: ({label}) disconnected unexpectedly");
                    MessageBoxRequested?.Invoke(T("drive_lost_title"),
                        string.Format(T("drive_lost_msg"), row.Letter, label), "warning");
                });
            }
            else
            {
                var permission = DriveService.HasWriteAccess(row.Letter);
                _dispatcher.Invoke(() => row.DrivePermission = permission);

                var expectedUnc = row.Config.UncPath.ToLowerInvariant();
                if (remote != null && remote.ToLowerInvariant() != expectedUnc)
                {
                    _dispatcher.Invoke(() =>
                        AddWarning($"mismatch_{row.Letter}",
                            string.Format(T("server_mismatch_status"),
                                row.Letter, remote, row.Config.Server, row.Config.Share)));
                }
            }
        })).ToArray();

        Task.WaitAll(tasks);
    }

    /// <summary>Adds or updates a warning in the warnings panel.</summary>
    private void AddWarning(string key, string message)
    {
        var existing = Warnings.FirstOrDefault(w => w.Key == key);
        if (existing != null)
        {
            existing.Message = $"\u26A0  {message}";
            return;
        }
        Warnings.Add(new WarningItem { Key = key, Message = $"\u26A0  {message}" });
    }

    /// <summary>Removes a warning by its key.</summary>
    private void ClearWarning(string key)
    {
        var item = Warnings.FirstOrDefault(w => w.Key == key);
        if (item != null) Warnings.Remove(item);
    }

    /// <summary>Dismisses a warning by key (user-triggered).</summary>
    private void DismissWarning(string? key)
    {
        if (key != null) ClearWarning(key);
    }

    /// <summary>Removes all warnings from the panel.</summary>
    private void ClearAllWarnings() => Warnings.Clear();

    /// <summary>Updates the status bar message and color.</summary>
    public void SetStatus(string message, string level = "info")
    {
        StatusMessage = message;
        StatusColor = level switch
        {
            "warn" => "#e65100",
            "ok" => "#2e7d32",
            _ => "#1565c0",
        };
    }

    /// <summary>Disposes the background monitor timer.</summary>
    public void Dispose()
    {
        _monitorTimer?.Dispose();
        _monitorTimer = null;
    }
}
