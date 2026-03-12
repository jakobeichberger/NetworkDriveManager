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
    /// <summary>Gets or sets the drive letter (A-Z).</summary>
    public string Letter { get; set; } = string.Empty;
    /// <summary>Gets or sets the file server hostname or IP address.</summary>
    public string Server { get; set; } = string.Empty;
    /// <summary>Gets or sets the SMB share name.</summary>
    public string Share { get; set; } = string.Empty;
    /// <summary>Gets or sets the friendly display name for the drive.</summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>Gets or sets whether the drive is hidden from the main window.</summary>
    public bool Hidden { get; set; }
    /// <summary>Gets a check-mark string for the hidden column display.</summary>
    public string HiddenDisplay => Hidden ? "\u2713" : "";
}

/// <summary>
/// ViewModel for the SettingsDialog.
/// </summary>
public class SettingsViewModel : ObservableObject
{
    /// <summary>Current UI language code used for translations.</summary>
    private readonly string _lang;

    /// <summary>Initializes the view model with the specified language and existing drive configurations.</summary>
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

    /// <summary>Returns the translated string for the given key.</summary>
    private string T(string key) => Translations.Get(_lang, key);

    /// <summary>Gets the translated settings dialog title.</summary>
    public string SettingsTitle => T("settings_title");
    /// <summary>Gets the translated drive letter column header.</summary>
    public string LetterHeader => T("drive_letter");
    /// <summary>Gets the translated server address column header.</summary>
    public string ServerHeader => T("server_address");
    /// <summary>Gets the translated share name column header.</summary>
    public string ShareHeader => T("share_name");
    /// <summary>Gets the translated label column header.</summary>
    public string LabelHeader => T("label");
    /// <summary>Gets the translated hidden column header.</summary>
    public string HiddenHeader => T("hidden");
    /// <summary>Gets the translated new drive button text.</summary>
    public string NewText => T("new_drive");
    /// <summary>Gets the translated add drive button text.</summary>
    public string AddText => T("add_drive");
    /// <summary>Gets the translated edit drive button text.</summary>
    public string EditText => T("edit_drive");
    /// <summary>Gets the translated remove drive button text.</summary>
    public string RemoveText => T("remove_drive");
    /// <summary>Gets the translated import drives button text.</summary>
    public string ImportText => T("import_drives");
    /// <summary>Gets the translated save button text.</summary>
    public string SaveText => T("save");
    /// <summary>Gets the translated cancel button text.</summary>
    public string CancelText => T("cancel");
    /// <summary>Gets the translated log section title.</summary>
    public string LogTitle => T("log_title");
    /// <summary>Gets the translated log refresh button text.</summary>
    public string LogRefreshText => T("log_refresh");
    /// <summary>Gets the translated log clear button text.</summary>
    public string LogClearText => T("log_clear");
    /// <summary>Gets the translated log export CSV button text.</summary>
    public string LogExportCsvText => T("log_export_csv");
    /// <summary>Gets the translated log file path label.</summary>
    public string LogFileLabel => string.Format(T("log_file_label"), ConfigService.LogFilePath);

    /// <summary>Backing field for <see cref="FormTitle"/>.</summary>
    private string _formTitle = string.Empty;
    /// <summary>Gets or sets the title of the add/edit form section.</summary>
    public string FormTitle
    {
        get => string.IsNullOrEmpty(_formTitle) ? T("form_add_title") : _formTitle;
        set => SetProperty(ref _formTitle, value);
    }

    /// <summary>Backing field for <see cref="FormLetter"/>.</summary>
    private string _formLetter = string.Empty;
    /// <summary>Gets or sets the drive letter form field value.</summary>
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

    /// <summary>Backing field for <see cref="FormServer"/>.</summary>
    private string _formServer = string.Empty;
    /// <summary>Gets or sets the server address form field value.</summary>
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

    /// <summary>Backing field for <see cref="FormShare"/>.</summary>
    private string _formShare = string.Empty;
    /// <summary>Gets or sets the share name form field value.</summary>
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

    /// <summary>Backing field for <see cref="FormLabel"/>.</summary>
    private string _formLabel = string.Empty;
    /// <summary>Gets or sets the label form field value.</summary>
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

    /// <summary>Backing field for <see cref="FormHidden"/>.</summary>
    private bool _formHidden;
    /// <summary>Gets or sets the hidden checkbox form field value.</summary>
    public bool FormHidden
    {
        get => _formHidden;
        set => SetProperty(ref _formHidden, value);
    }

    /// <summary>Gets the validation hint text for the drive letter field.</summary>
    public string LetterHint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FormLetter)) return T("letter_hint");
            return FormLetter.Trim().Length == 1 && char.IsLetter(FormLetter.Trim()[0])
                ? T("letter_hint_valid") : T("letter_hint_invalid");
        }
    }
    /// <summary>Gets the color for the drive letter validation hint text.</summary>
    public string LetterHintColor => string.IsNullOrWhiteSpace(FormLetter) ? "#6b7280"
        : (FormLetter.Trim().Length == 1 && char.IsLetter(FormLetter.Trim()[0]) ? "#2e7d32" : "#b71c1c");
    /// <summary>Gets the color for the drive letter validation indicator.</summary>
    public string LetterIndicatorColor => LetterHintColor;

    /// <summary>Gets the validation hint text for the server field.</summary>
    public string ServerHint => string.IsNullOrWhiteSpace(FormServer) ? T("server_hint") : T("server_hint_valid");
    /// <summary>Gets the color for the server validation hint text.</summary>
    public string ServerHintColor => string.IsNullOrWhiteSpace(FormServer) ? "#6b7280" : "#2e7d32";
    /// <summary>Gets the color for the server validation indicator.</summary>
    public string ServerIndicatorColor => ServerHintColor;

    /// <summary>Gets the validation hint text for the share field.</summary>
    public string ShareHint => string.IsNullOrWhiteSpace(FormShare) ? T("share_hint") : T("share_hint_valid");
    /// <summary>Gets the color for the share validation hint text.</summary>
    public string ShareHintColor => string.IsNullOrWhiteSpace(FormShare) ? "#6b7280" : "#2e7d32";
    /// <summary>Gets the color for the share validation indicator.</summary>
    public string ShareIndicatorColor => ShareHintColor;

    /// <summary>Gets the validation hint text for the label field.</summary>
    public string LabelHint => string.IsNullOrWhiteSpace(FormLabel) ? T("label_hint") : T("label_hint_valid");
    /// <summary>Gets the color for the label validation hint text.</summary>
    public string LabelHintColor => string.IsNullOrWhiteSpace(FormLabel) ? "#6b7280" : "#2e7d32";
    /// <summary>Gets the color for the label validation indicator.</summary>
    public string LabelIndicatorColor => LabelHintColor;

    /// <summary>Gets the observable collection of drive configuration rows.</summary>
    public ObservableCollection<SettingsDriveRow> Drives { get; } = new();

    /// <summary>Backing field for <see cref="SelectedDrive"/>.</summary>
    private SettingsDriveRow? _selectedDrive;
    /// <summary>Gets or sets the currently selected drive in the data grid.</summary>
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

    /// <summary>Gets or sets the index of the drive being edited, or -1 if adding a new drive.</summary>
    public int EditingIndex { get; set; } = -1;

    /// <summary>Resets all form fields and editing state to defaults.</summary>
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

    /// <summary>Converts the drive rows to a list of <see cref="DriveConfig"/> for saving.</summary>
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
    /// <summary>Reference to the main window ViewModel.</summary>
    private readonly MainViewModel _mainVm;
    /// <summary>Settings dialog ViewModel instance.</summary>
    private readonly SettingsViewModel _vm;

    /// <summary>Initializes the settings dialog and loads the current drive configuration.</summary>
    public SettingsDialog(MainViewModel mainVm)
    {
        InitializeComponent();
        _mainVm = mainVm;

        var drives = ConfigService.LoadConfig();
        _vm = new SettingsViewModel(mainVm.Lang, drives);
        DataContext = _vm;

        RefreshLog();
    }

    /// <summary>Clears the form for a new drive entry.</summary>
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

    /// <summary>Validates the form and adds a new drive to the list.</summary>
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

    /// <summary>Validates the form and updates the currently selected drive.</summary>
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

    /// <summary>Removes the selected drive from the list after user confirmation.</summary>
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

    /// <summary>Imports drive configurations from a batch (.bat/.cmd) file.</summary>
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

    /// <summary>Saves the drive configuration and closes the dialog.</summary>
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

    /// <summary>Closes the dialog without saving changes.</summary>
    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>Reloads error and warning log entries into the log text box.</summary>
    private void RefreshLog()
    {
        var entries = LogService.ReadErrorWarningEntries();
        var lang = _mainVm.Lang;
        LogTextBox.Text = entries.Count == 0
            ? Translations.Get(lang, "log_empty")
            : string.Join(Environment.NewLine, entries);
        LogTextBox.ScrollToEnd();
    }

    /// <summary>Handles the log refresh button click event.</summary>
    private void OnLogRefresh(object sender, RoutedEventArgs e) => RefreshLog();

    /// <summary>Clears all log files after user confirmation.</summary>
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

    /// <summary>Exports log entries to a CSV file chosen by the user.</summary>
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
