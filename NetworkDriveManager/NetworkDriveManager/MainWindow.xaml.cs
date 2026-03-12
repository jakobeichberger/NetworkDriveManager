using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NetworkDriveManager.Services;
using NetworkDriveManager.ViewModels;
using NetworkDriveManager.Views;

namespace NetworkDriveManager;

/// <summary>
/// Main application window — code-behind for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();

        _vm = new MainViewModel();
        DataContext = _vm;

        // Wire up events that require View interaction
        _vm.SettingsRequested += OnSettingsRequested;
        _vm.HelpRequested += OnHelpRequested;
        _vm.MessageBoxRequested += OnMessageBoxRequested;

        // Try to load the logo
        LoadLogo();

        // Sync the initial password if credentials were loaded
        if (!string.IsNullOrEmpty(_vm.Password))
            PasswordBox.Password = _vm.Password;

        Closed += (_, _) => _vm.Dispose();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.Password = ((PasswordBox)sender).Password;
    }

    private void OnSettingsRequested()
    {
        var dialog = new SettingsDialog(_vm) { Owner = this };
        dialog.ShowDialog();
    }

    private void OnHelpRequested()
    {
        var dialog = new HelpDialog(_vm.Lang) { Owner = this };
        dialog.ShowDialog();
    }

    private void OnMessageBoxRequested(string title, string message, string type)
    {
        var icon = type switch
        {
            "error" => MessageBoxImage.Error,
            "warning" => MessageBoxImage.Warning,
            _ => MessageBoxImage.Information,
        };
        MessageBox.Show(this, message, title, MessageBoxButton.OK, icon);
    }

    private void LoadLogo()
    {
        try
        {
            // Look for logo.png next to the executable, or in the project root
            var candidates = new[]
            {
                Path.Combine(ConfigService.GetRuntimeDir(), "logo.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "logo.png"),
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
                    bitmap.DecodePixelHeight = 72;
                    bitmap.EndInit();
                    LogoImage.Source = bitmap;
                    LogService.Debug($"Logo loaded from {path}");
                    return;
                }
            }
            LogService.Debug("Logo file not found — header shown without logo");
        }
        catch (Exception ex)
        {
            LogService.Warning($"Could not load logo: {ex.Message}");
        }
    }
}
