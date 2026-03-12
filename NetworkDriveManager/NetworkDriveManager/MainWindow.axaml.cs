using Avalonia.Controls;
using Avalonia.Interactivity;
using NetworkDriveManager.Services;
using NetworkDriveManager.ViewModels;
using NetworkDriveManager.Views;

namespace NetworkDriveManager;

/// <summary>
/// Main application window — code-behind for MainWindow.axaml.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>The main ViewModel bound to this window.</summary>
    private readonly MainViewModel _vm;

    /// <summary>
    /// Initializes the main window, creates the ViewModel, and wires up events.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        _vm = new MainViewModel();
        DataContext = _vm;

        // Wire up events that require View interaction
        _vm.SettingsRequested += OnSettingsRequested;
        _vm.HelpRequested += OnHelpRequested;
        _vm.MessageBoxRequested += OnMessageBoxRequested;

        Closed += (_, _) => _vm.Dispose();
    }

    /// <summary>
    /// Opens the Settings dialog as a modal window.
    /// </summary>
    private async void OnSettingsRequested()
    {
        var dialog = new SettingsDialog(_vm);
        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// Opens the Help dialog as a modal window.
    /// </summary>
    private async void OnHelpRequested()
    {
        var dialog = new HelpDialog(_vm.Lang);
        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// Displays a message box with the appropriate icon based on the message type.
    /// </summary>
    private async void OnMessageBoxRequested(string title, string message, string type)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
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

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Padding = new Avalonia.Thickness(20, 6),
        };
        okButton.Click += (_, _) => dialog.Close();
        panel.Children.Add(okButton);

        dialog.Content = panel;
        await dialog.ShowDialog(this);
    }
}
