using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using NetworkDriveManager.Helpers;
using NetworkDriveManager.Models;
using NetworkDriveManager.Services;
using NetworkDriveManager.ViewModels;

namespace NetworkDriveManager.Views;

/// <summary>
/// ViewModel for the Settings dialog's data grid rows.
/// </summary>
public class SettingsDriveRow : ObservableObject
{
    public string Letter { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Share { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Hidden { get; set; }
    public string HiddenDisplay => Hidden ? "\u2713" : "";
}

/// <summary>
/// ViewModel for the SettingsDialog.
/// </summary>
public class SettingsViewModel : ObservableObject
{
    private readonly string _lang;

    public SettingsViewModel(string lang, List<DriveConfig> drives)
    {
        _lang = lang;
        foreach (var d in drives)
            Drives.Add(new SettingsDriveRow
            {
                Letter = d.Letter, Server = d.Server, Share = d.Share,
                Label = d.Label, Hidden = d.Hidden
            });
    }

    private string T(string key) => Translations.Get(_lang, key);

    // Translated properties
    public string SettingsTitle => T("settings_title");
    public string LetterHeader => T("drive_letter");
    public string ServerHeader => T("server_address");
    public string ShareHeader => T("share_name");
    public string LabelHeader => T("label");
    public string HiddenHeader => T("hidden");
    public string NewText => T("new_drive");
    public string AddText => T("add_drive");
    public string EditText => T("edit_drive");
    public string RemoveText => T("remove_drive");
    public string ImportText => T("import_drives");
    public string SaveText => T("save");
    public string CancelText => T("cancel");
    public string LogTitle => T("log_title");
    public string LogRefreshText => T("log_refresh");
    public string LogClearText => T("log_clear");
    public string LogExportCsvText => T("log_export_csv");
    public string LogFileLabel => string.Format(T("log_file_label"), ConfigService.LogFilePath);

    // Form
    private string _formTitle = string.Empty;
    public string FormTitle
    {
        get => string.IsNullOrEmpty(_formTitle) ? T("form_add_title") : _formTitle;
        set => SetProperty(ref _formTitle, value);
    }

    private string _formLetter = string.Empty;
    public string FormLetter
    {
        get => _formLetter;
        set
        {
            if (SetProperty(ref _formLetter, value))
            {
                OnPropertyChanged(nameof(LetterHint));
                OnPropertyChanged(nameof(LetterHintColor));
                OnPropertyChanged(nameof(LetterIndicatorColor));
            }
        }
    }

    private string _formServer = string.Empty;
    public string FormServer
    {
        get => _formServer;
        set
        {
            if (SetProperty(ref _formServer, value))
            {
                OnPropertyChanged(nameof(ServerHint));
                OnPropertyChanged(nameof(ServerHintColor));
                OnPropertyChanged(nameof(ServerIndicatorColor));
            }
        }
    }

    private string _formShare = string.Empty;
    public string FormShare
    {
        get => _formShare;
        set
        {
            if (SetProperty(ref _formShare, value))
            {
                OnPropertyChanged(nameof(ShareHint));
                OnPropertyChanged(nameof(ShareHintColor));
                OnPropertyChanged(nameof(ShareIndicatorColor));
            }
        }
    }

    private string _formLabel = string.Empty;
    public string FormLabel
    {
        get => _formLabel;
        set
        {
            if (SetProperty(ref _formLabel, value))
            {
                OnPropertyChanged(nameof(LabelHint));
                OnPropertyChanged(nameof(LabelHintColor));
                OnPropertyChanged(nameof(LabelIndicatorColor));
            }
        }
    }

    private bool _formHidden;
    public bool FormHidden
    {
        get => _formHidden;
        set => SetProperty(ref _formHidden, value);
    }

    // Validation hints
    public string LetterHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FormLetter)) return T("letter_hint");
            return FormLetter.Trim().Length == 1 && char.IsLetter(FormLetter.Trim()[0])
                ? T("letter_hint_valid") : T("letter_hint_invalid");
        }
    }
    public string LetterHintColor => string.IsNullOrWhiteSpace(FormLetter) ? "#6b7280"
        : (FormLetter.Trim().Length == 1 && char.IsLetter(FormLetter.Trim()[0]) ? "#2e7d32" : "#b71c1c");
    public string LetterIndicatorColor => LetterHintColor;

    public string ServerHint => string.IsNullOrWhiteSpace(FormServer) ? T("server_hint") : T("server_hint_valid");
    public string ServerHintColor => string.IsNullOrWhiteSpace(FormServer) ? "#6b7280" : "#2e7d32";
    public string ServerIndicatorColor => ServerHintColor;

    public string ShareHint => string.IsNullOrWhiteSpace(FormShare) ? T("share_hint") : T("share_hint_valid");
    public string ShareHintColor => string.IsNullOrWhiteSpace(FormShare) ? "#6b7280" : "#2e7d32";
    public string ShareIndicatorColor => ShareHintColor;

    public string LabelHint => string.IsNullOrWhiteSpace(FormLabel) ? T("label_hint") : T("label_hint_valid");
    public string LabelHintColor => string.IsNullOrWhiteSpace(FormLabel) ? "#6b7280" : "#2e7d32";
    public string LabelIndicatorColor => LabelHintColor;

    // Drive list
    public ObservableCollection<SettingsDriveRow> Drives { get; } = new();

    private SettingsDriveRow? _selectedDrive;
    public SettingsDriveRow? SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (SetProperty(ref _selectedDrive, value) && value != null)
            {
                FormLetter = value.Letter;
                FormServer = value.Server;
                FormShare = value.Share;
                FormLabel = value.Label;
                FormHidden = value.Hidden;
                EditingIndex = Drives.IndexOf(value);
                FormTitle = string.Format(T("form_edit_title"), value.Letter);
            }
        }
    }

    public int EditingIndex { get; set; } = -1;

    public void ClearForm()
    {
        FormLetter = string.Empty;
        FormServer = string.Empty;
        FormShare = string.Empty;
        FormLabel = string.Empty;
        FormHidden = false;
        EditingIndex = -1;
        FormTitle = T("form_add_title");
        SelectedDrive = null;
    }

    public List<DriveConfig> ToDriveConfigs()
    {
        return Drives.Select(d => new DriveConfig
        {
            Letter = d.Letter,
            Server = d.Server,
            Share = d.Share,
            Label = d.Label,
            Hidden = d.Hidden,
        }).ToList();
    }
}

/// <summary>
/// Settings dialog for managing drive configuration.
/// </summary>
public partial class SettingsDialog : Window
{
    private readonly MainViewModel _mainVm;
    private readonly SettingsViewModel _vm;

    public SettingsDialog(MainViewModel mainVm)
    {
        InitializeComponent();
        _mainVm = mainVm;

        var drives = ConfigService.LoadConfig();
        _vm = new SettingsViewModel(mainVm.Lang, drives);
        DataContext = _vm;

        RefreshLog();
    }

    private void OnNew(object sender, RoutedEventArgs e)
    {
        _vm.ClearForm();
        DriveGrid.SelectedItem = null;
    }

    /// <summary>
    /// Validate form fields and check for duplicate drive letters.
    /// Returns the validated (letter, server, share, label) or null if validation fails.
    /// </summary>
    private (string Letter, string Server, string Share, string Label)? ValidateForm(int excludeIndex = -1)
    {
        var letter = _vm.FormLetter.Trim().ToUpper();
        var server = _vm.FormServer.Trim();
        var share = _vm.FormShare.Trim();
        var label = _vm.FormLabel.Trim();
        var lang = _mainVm.Lang;

        if (string.IsNullOrEmpty(letter) || string.IsNullOrEmpty(server) ||
            string.IsNullOrEmpty(share) || string.IsNullOrEmpty(label))
        {
            MessageBox.Show(this, Translations.Get(lang, "drive_fields_required"),
                Translations.Get(lang, "error"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (letter.Length != 1 || !char.IsLetter(letter[0]))
        {
            MessageBox.Show(this, Translations.Get(lang, "drive_letter_invalid"),
                Translations.Get(lang, "error"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        for (int i = 0; i < _vm.Drives.Count; i++)
        {
            if (i != excludeIndex && _vm.Drives[i].Letter.Equals(letter, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, string.Format(Translations.Get(lang, "drive_letter_duplicate"), letter),
                    Translations.Get(lang, "error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        return (letter, server, share, label);
    }

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        var validated = ValidateForm();
        if (validated is not { } v) return;

        _vm.Drives.Add(new SettingsDriveRow
        {
            Letter = v.Letter, Server = v.Server, Share = v.Share,
            Label = v.Label, Hidden = _vm.FormHidden
        });
        _vm.ClearForm();
    }

    private void OnEdit(object sender, RoutedEventArgs e)
    {
        if (_vm.EditingIndex < 0 || _vm.EditingIndex >= _vm.Drives.Count) return;

        var validated = ValidateForm(excludeIndex: _vm.EditingIndex);
        if (validated is not { } v) return;

        _vm.Drives[_vm.EditingIndex] = new SettingsDriveRow
        {
            Letter = v.Letter, Server = v.Server, Share = v.Share,
            Label = v.Label, Hidden = _vm.FormHidden
        };
        _vm.ClearForm();
    }

    private void OnRemove(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedDrive == null) return;
        var lang = _mainVm.Lang;
        var result = MessageBox.Show(this,
            string.Format(Translations.Get(lang, "confirm_remove_msg"),
                _vm.SelectedDrive.Letter, _vm.SelectedDrive.Label),
            Translations.Get(lang, "confirm_remove_title"),
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _vm.Drives.Remove(_vm.SelectedDrive);
            _vm.ClearForm();
        }
    }

    private void OnImport(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        var dlg = new OpenFileDialog
        {
            Title = Translations.Get(lang, "import_select_file"),
            Filter = "Batch files (*.bat;*.cmd)|*.bat;*.cmd|All files (*.*)|*.*",
        };

        if (dlg.ShowDialog(this) != true) return;

        try
        {
            var content = File.ReadAllText(dlg.FileName);
            var (imported, skipped) = DriveService.ParseBatDrives(content);

            if (imported.Count == 0)
            {
                MessageBox.Show(this, Translations.Get(lang, "import_drives_no_drives"),
                    Translations.Get(lang, "import_drives_title"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var msg = string.Format(Translations.Get(lang, "import_drives_confirm"), imported.Count);
            if (skipped > 0)
                msg += "\n\n" + string.Format(Translations.Get(lang, "import_drives_skipped"), skipped);

            if (MessageBox.Show(this, msg, Translations.Get(lang, "import_drives_title"),
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _vm.Drives.Clear();
            foreach (var d in imported)
                _vm.Drives.Add(new SettingsDriveRow
                {
                    Letter = d.Letter, Server = d.Server, Share = d.Share,
                    Label = d.Label, Hidden = d.Hidden
                });
            _vm.ClearForm();

            var successMsg = string.Format(Translations.Get(lang, "import_drives_success"), imported.Count);
            if (skipped > 0)
                successMsg += "\n" + string.Format(Translations.Get(lang, "import_drives_skipped"), skipped);

            MessageBox.Show(this, successMsg, Translations.Get(lang, "import_drives_title"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, string.Format(Translations.Get(lang, "import_drives_error"), ex.Message),
                Translations.Get(lang, "error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        try
        {
            ConfigService.SaveConfig(_vm.ToDriveConfigs());
            MessageBox.Show(this, Translations.Get(lang, "settings_saved_msg"),
                Translations.Get(lang, "settings_saved"),
                MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
            _mainVm.ReloadDrives();
        }
        catch (Exception ex)
        {
            LogService.Error($"Failed to save drive configuration: {ex.Message}");
            MessageBox.Show(this,
                string.Format(Translations.Get(lang, "settings_save_failed"), ex.Message),
                Translations.Get(lang, "error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    // ── Log section ──────────────────────────────────────────────────
    private void RefreshLog()
    {
        var entries = LogService.ReadErrorWarningEntries();
        var lang = _mainVm.Lang;
        LogTextBox.Text = entries.Count == 0
            ? Translations.Get(lang, "log_empty")
            : string.Join(Environment.NewLine, entries);
        LogTextBox.ScrollToEnd();
    }

    private void OnLogRefresh(object sender, RoutedEventArgs e) => RefreshLog();

    private void OnLogClear(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        if (MessageBox.Show(this, Translations.Get(lang, "log_clear_confirm_msg"),
            Translations.Get(lang, "log_clear_confirm_title"),
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            LogService.ClearLog();
            RefreshLog();
        }
    }

    private void OnLogExportCsv(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        var dlg = new SaveFileDialog
        {
            Title = Translations.Get(lang, "log_export_csv_title"),
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv",
        };

        if (dlg.ShowDialog(this) != true) return;

        try
        {
            var entries = LogService.ReadErrorWarningEntries();
            var logPattern = new Regex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\s+\[(\w+)\]\s+(.*)");

            using var writer = new StreamWriter(dlg.FileName, false, new System.Text.UTF8Encoding(true));
            writer.WriteLine("Timestamp,Level,Message");
            foreach (var line in entries)
            {
                var match = logPattern.Match(line);
                if (match.Success)
                    writer.WriteLine($"\"{match.Groups[1].Value}\",\"{match.Groups[2].Value}\",\"{match.Groups[3].Value.Replace("\"", "\"\"")}\"");
                else
                    writer.WriteLine($"\"\",,\"{line.Replace("\"", "\"\"")}\"");
            }

            LogService.Info($"Log exported to CSV: {dlg.FileName}");
            MessageBox.Show(this,
                string.Format(Translations.Get(lang, "log_export_csv_success"), dlg.FileName),
                Translations.Get(lang, "log_export_csv_title"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LogService.Error($"Failed to export log to CSV: {ex.Message}");
            MessageBox.Show(this,
                string.Format(Translations.Get(lang, "log_export_csv_error"), ex.Message),
                Translations.Get(lang, "error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
