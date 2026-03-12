using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NetworkDriveManager.Helpers;
using NetworkDriveManager.Models;
using NetworkDriveManager.Services;

namespace NetworkDriveManager.ViewModels;

/// <summary>
/// Represents a single drive row in the main window's drive list.
/// </summary>
public class DriveRowViewModel : ObservableObject
{
    public DriveConfig Config { get; }

    public string Letter => Config.Letter;
    public string DisplayPath => $"{Config.Label}  ({Config.UncPath})";

    private bool _isConnected;
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
            }
        }
    }

    // null = not checked, true = reachable, false = unreachable
    private bool? _serverReachable;
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

    private bool _isCheckingServer;
    public bool IsCheckingServer
    {
        get => _isCheckingServer;
        set => SetProperty(ref _isCheckingServer, value);
    }

    private string _lang = "de";
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

    public string StatusText => IsConnected
        ? Translations.Get(Lang, "connected")
        : Translations.Get(Lang, "disconnected");

    public string StatusColor => IsConnected ? "#2e7d32" : "#b71c1c";

    public string ToggleButtonText => IsConnected
        ? Translations.Get(Lang, "disconnect")
        : Translations.Get(Lang, "connect");

    public string ToggleButtonColor => IsConnected ? "#2e7d32" : "#b71c1c";

    public string ServerCheckText
    {
        get
        {
            if (IsCheckingServer) return Translations.Get(Lang, "working");
            return ServerReachable switch
            {
                true => Translations.Get(Lang, "server_reachable"),
                false => Translations.Get(Lang, "server_unreachable_short"),
                null => Translations.Get(Lang, "server_check_btn"),
            };
        }
    }

    public string ServerCheckColor
    {
        get
        {
            if (IsCheckingServer) return "#78909c";
            return ServerReachable switch
            {
                true => "#2e7d32",
                false => "#b71c1c",
                null => "#78909c",
            };
        }
    }

    public DriveRowViewModel(DriveConfig config) => Config = config;
}

/// <summary>
/// Represents a warning in the warnings panel.
/// </summary>
public class WarningItem : ObservableObject
{
    public string Key { get; set; } = string.Empty;

    private string _message = string.Empty;
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
    private readonly Dispatcher _dispatcher;
    private System.Threading.Timer? _monitorTimer;
    private bool _busy;

    /// <summary>How often (seconds) the background monitor checks connected drives.</summary>
    private const int MonitorIntervalSeconds = 30;

    // ── Language ──────────────────────────────────────────────────────
    private string _lang = "de";
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

    public string T(string key) => Translations.Get(Lang, key);

    // ── Translated properties ────────────────────────────────────────
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

    // ── Credentials ──────────────────────────────────────────────────
    private string _username = string.Empty;
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

    public string UsernameHintText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Username)) return T("username_hint");
            return Username.Contains('\\') ? T("username_hint_valid") : T("username_hint_invalid");
        }
    }

    public string UsernameHintColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Username)) return "#6b7280";
            return Username.Contains('\\') ? "#2e7d32" : "#b71c1c";
        }
    }

    public string UsernameIndicatorColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Username)) return "#6b7280";
            return Username.Contains('\\') ? "#2e7d32" : "#b71c1c";
        }
    }

    public string PasswordHintText
    {
        get
        {
            if (string.IsNullOrEmpty(Password)) return T("password_hint");
            return T("password_hint_valid");
        }
    }

    public string PasswordHintColor =>
        string.IsNullOrEmpty(Password) ? "#6b7280" : "#2e7d32";

    public string PasswordIndicatorColor =>
        string.IsNullOrEmpty(Password) ? "#6b7280" : "#2e7d32";

    // ── Drive rows ───────────────────────────────────────────────────
    public ObservableCollection<DriveRowViewModel> DriveRows { get; } = new();

    // ── Warnings ─────────────────────────────────────────────────────
    public ObservableCollection<WarningItem> Warnings { get; } = new();
    public bool HasWarnings => Warnings.Count > 0;

    // ── Status bar ───────────────────────────────────────────────────
    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private string _statusColor = "#1565c0";
    public string StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    // ── Busy state ───────────────────────────────────────────────────
    public bool IsBusy
    {
        get => _busy;
        set
        {
            if (SetProperty(ref _busy, value))
                OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !_busy;

    // ── Commands ─────────────────────────────────────────────────────
    public ICommand ToggleLanguageCommand { get; }
    public ICommand SaveCredentialsCommand { get; }
    public ICommand ConnectAllCommand { get; }
    public ICommand DisconnectAllCommand { get; }
    public ICommand RefreshStatusCommand { get; }
    public ICommand ToggleDriveCommand { get; }
    public ICommand CheckServerCommand { get; }
    public ICommand DismissWarningCommand { get; }
    // These are handled by the View (open dialog windows)
    public ICommand OpenSettingsCommand { get; }
    public ICommand OpenHelpCommand { get; }

    // Events for the View to handle
    public event Action? SettingsRequested;
    public event Action? HelpRequested;
    public event Action<string, string, string>? MessageBoxRequested; // title, msg, type

    public MainViewModel()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;

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

    // ── Language ──────────────────────────────────────────────────────
    private void ToggleLanguage()
    {
        Lang = Lang == "de" ? "en" : "de";
    }

    // ── Credentials ──────────────────────────────────────────────────
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

    // ── Drive loading ────────────────────────────────────────────────
    public void LoadDrives()
    {
        DriveRows.Clear();
        var drives = ConfigService.LoadConfig();
        foreach (var d in drives.Where(d => !d.Hidden))
        {
            DriveRows.Add(new DriveRowViewModel(d) { Lang = Lang });
        }
    }

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

    // ── Drive operations ─────────────────────────────────────────────
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

    private async Task RefreshAllStatusesAsync()
    {
        var tasks = DriveRows.Select(row => Task.Run(() =>
        {
            var (connected, remote) = DriveService.GetDriveInfo(row.Letter);
            _dispatcher.Invoke(() =>
            {
                row.IsConnected = connected;
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
                    _dispatcher.Invoke(() =>
                    {
                        if (ok) row.IsConnected = true;
                        else MessageBoxRequested?.Invoke(T("error"),
                            string.Format(T("connect_failed"), row.Letter, msg), "error");
                    });
                }
            }
            finally
            {
                _dispatcher.Invoke(() => IsBusy = false);
            }
        });
    }

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
                        _dispatcher.Invoke(() => row.IsConnected = true);
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

    // ── Server checks ────────────────────────────────────────────────
    private void OnCheckServer(DriveRowViewModel? row)
    {
        if (row == null) return;
        var server = row.Config.Server;

        // Set all drives on same server to "checking"
        foreach (var r in DriveRows.Where(r => r.Config.Server == server))
            r.IsCheckingServer = true;

        Task.Run(() =>
        {
            var reachable = ServerService.IsServerReachable(server);
            _dispatcher.Invoke(() =>
            {
                foreach (var r in DriveRows.Where(r => r.Config.Server == server))
                {
                    r.ServerReachable = reachable;
                    r.IsCheckingServer = false;
                }
            });
        });
    }

    private async Task RefreshAllServerPingsAsync()
    {
        // Group by server to avoid duplicate checks
        var serverGroups = DriveRows.GroupBy(r => r.Config.Server).ToList();

        _dispatcher.Invoke(() =>
        {
            foreach (var row in DriveRows)
                row.IsCheckingServer = true;
        });

        var tasks = serverGroups.Select(g => Task.Run(() =>
        {
            var reachable = ServerService.IsServerReachable(g.Key);
            _dispatcher.Invoke(() =>
            {
                foreach (var row in g)
                {
                    row.ServerReachable = reachable;
                    row.IsCheckingServer = false;
                }
            });
        })).ToArray();

        await Task.WhenAll(tasks);
    }

    // ── Background monitor ───────────────────────────────────────────
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
                    var label = row.Config.Label;
                    LogService.Warning($"Drive {row.Letter}: ({label}) disconnected unexpectedly");
                    MessageBoxRequested?.Invoke(T("drive_lost_title"),
                        string.Format(T("drive_lost_msg"), row.Letter, label), "warning");
                });
            }
            else
            {
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

    // ── Warnings ─────────────────────────────────────────────────────
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

    private void ClearWarning(string key)
    {
        var item = Warnings.FirstOrDefault(w => w.Key == key);
        if (item != null) Warnings.Remove(item);
    }

    private void DismissWarning(string? key)
    {
        if (key != null) ClearWarning(key);
    }

    private void ClearAllWarnings() => Warnings.Clear();

    // ── Status bar ───────────────────────────────────────────────────
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

    // ── Cleanup ──────────────────────────────────────────────────────
    public void Dispose()
    {
        _monitorTimer?.Dispose();
        _monitorTimer = null;
    }
}
