using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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

    public ObservableCollection<SettingsDriveRow> Drives { get; } = [];

    private SettingsDriveRow? _selectedDrive;
    public SettingsDriveRow? SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (SetProperty(ref _selectedDrive, value) && value is not null)
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

    public List<DriveConfig> ToDriveConfigs() =>
        Drives.Select(d => new DriveConfig
        {
            Letter = d.Letter,
            Server = d.Server,
            Share = d.Share,
            Label = d.Label,
            Hidden = d.Hidden,
        }).ToList();
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

    /// <summary>Required by Avalonia XAML loader.</summary>
    public SettingsDialog() : this(new MainViewModel()) { }

    private void OnNew(object sender, RoutedEventArgs e)
    {
        _vm.ClearForm();
        DriveGrid.SelectedItem = null;
    }

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
            ShowMessage(Translations.Get(lang, "error"), Translations.Get(lang, "drive_fields_required"));
            return null;
        }

        if (letter.Length != 1 || !char.IsLetter(letter[0]))
        {
            ShowMessage(Translations.Get(lang, "error"), Translations.Get(lang, "drive_letter_invalid"));
            return null;
        }

        for (var i = 0; i < _vm.Drives.Count; i++)
        {
            if (i != excludeIndex && _vm.Drives[i].Letter.Equals(letter, StringComparison.OrdinalIgnoreCase))
            {
                ShowMessage(Translations.Get(lang, "error"),
                    string.Format(Translations.Get(lang, "drive_letter_duplicate"), letter));
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

    private async void OnRemove(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedDrive is null) return;
        var lang = _mainVm.Lang;
        var msg = string.Format(Translations.Get(lang, "confirm_remove_msg"),
            _vm.SelectedDrive.Letter, _vm.SelectedDrive.Label);

        var confirmed = await ShowConfirmation(Translations.Get(lang, "confirm_remove_title"), msg);
        if (confirmed)
        {
            _vm.Drives.Remove(_vm.SelectedDrive);
            _vm.ClearForm();
        }
    }

    private async void OnImport(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Translations.Get(lang, "import_select_file"),
            FileTypeFilter =
            [
                new FilePickerFileType("Batch files") { Patterns = ["*.bat", "*.cmd"] },
                new FilePickerFileType("All files") { Patterns = ["*"] },
            ],
            AllowMultiple = false,
        });

        if (files.Count == 0) return;

        try
        {
            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var (imported, skipped) = DriveService.ParseBatDrives(content);

            if (imported.Count == 0)
            {
                ShowMessage(Translations.Get(lang, "import_drives_title"),
                    Translations.Get(lang, "import_drives_no_drives"));
                return;
            }

            var confirmMsg = string.Format(Translations.Get(lang, "import_drives_confirm"), imported.Count);
            if (skipped > 0)
                confirmMsg += "\n\n" + string.Format(Translations.Get(lang, "import_drives_skipped"), skipped);

            var confirmed = await ShowConfirmation(Translations.Get(lang, "import_drives_title"), confirmMsg);
            if (!confirmed) return;

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

            ShowMessage(Translations.Get(lang, "import_drives_title"), successMsg);
        }
        catch (Exception ex)
        {
            ShowMessage(Translations.Get(lang, "error"),
                string.Format(Translations.Get(lang, "import_drives_error"), ex.Message));
        }
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        try
        {
            ConfigService.SaveConfig(_vm.ToDriveConfigs());
            ShowMessage(Translations.Get(lang, "settings_saved"), Translations.Get(lang, "settings_saved_msg"));
            Close();
            _mainVm.ReloadDrives();
        }
        catch (Exception ex)
        {
            LogService.Error($"Failed to save drive configuration: {ex.Message}");
            ShowMessage(Translations.Get(lang, "error"),
                string.Format(Translations.Get(lang, "settings_save_failed"), ex.Message));
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e) => Close();

    private void RefreshLog()
    {
        var entries = LogService.ReadErrorWarningEntries();
        var lang = _mainVm.Lang;
        LogTextBox.Text = entries.Count == 0
            ? Translations.Get(lang, "log_empty")
            : string.Join(Environment.NewLine, entries);
    }

    private void OnLogRefresh(object sender, RoutedEventArgs e) => RefreshLog();

    private async void OnLogClear(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        var confirmed = await ShowConfirmation(
            Translations.Get(lang, "log_clear_confirm_title"),
            Translations.Get(lang, "log_clear_confirm_msg"));

        if (confirmed)
        {
            LogService.ClearLog();
            RefreshLog();
        }
    }

    private async void OnLogExportCsv(object sender, RoutedEventArgs e)
    {
        var lang = _mainVm.Lang;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Translations.Get(lang, "log_export_csv_title"),
            DefaultExtension = "csv",
            FileTypeChoices =
            [
                new FilePickerFileType("CSV files") { Patterns = ["*.csv"] },
                new FilePickerFileType("All files") { Patterns = ["*"] },
            ],
        });

        if (file is null) return;

        try
        {
            var entries = LogService.ReadErrorWarningEntries();
            var logPattern = new Regex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\s+\[(\w+)\]\s+(.*)");

            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(true));
            await writer.WriteLineAsync("Timestamp,Level,Message");

            foreach (var line in entries)
            {
                var match = logPattern.Match(line);
                if (match.Success)
                    await writer.WriteLineAsync(
                        $"\"{match.Groups[1].Value}\",\"{match.Groups[2].Value}\",\"{match.Groups[3].Value.Replace("\"", "\"\"")}\"");
                else
                    await writer.WriteLineAsync($"\"\",,\"{line.Replace("\"", "\"\"")}\"");
            }

            LogService.Info($"Log exported to CSV: {file.Name}");
            ShowMessage(Translations.Get(lang, "log_export_csv_title"),
                string.Format(Translations.Get(lang, "log_export_csv_success"), file.Name));
        }
        catch (Exception ex)
        {
            LogService.Error($"Failed to export log to CSV: {ex.Message}");
            ShowMessage(Translations.Get(lang, "error"),
                string.Format(Translations.Get(lang, "log_export_csv_error"), ex.Message));
        }
    }

    /// <summary>Shows a simple message dialog.</summary>
    private async void ShowMessage(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 20),
        });
        var okBtn = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Padding = new Avalonia.Thickness(20, 6),
        };
        okBtn.Click += (_, _) => dialog.Close();
        panel.Children.Add(okBtn);
        dialog.Content = panel;
        await dialog.ShowDialog(this);
    }

    /// <summary>Shows a Yes/No confirmation dialog and returns the user's choice.</summary>
    private async Task<bool> ShowConfirmation(string title, string message)
    {
        var result = false;
        var dialog = new Window
        {
            Title = title,
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 20),
        });

        var btnPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
        var noBtn = new Button { Content = "No", Margin = new Avalonia.Thickness(0, 0, 8, 0), Padding = new Avalonia.Thickness(20, 6) };
        var yesBtn = new Button { Content = "Yes", Padding = new Avalonia.Thickness(20, 6) };
        noBtn.Click += (_, _) => { result = false; dialog.Close(); };
        yesBtn.Click += (_, _) => { result = true; dialog.Close(); };
        btnPanel.Children.Add(noBtn);
        btnPanel.Children.Add(yesBtn);
        panel.Children.Add(btnPanel);

        dialog.Content = panel;
        await dialog.ShowDialog(this);
        return result;
    }
}
