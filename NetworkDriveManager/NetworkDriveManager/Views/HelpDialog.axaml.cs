using Avalonia.Controls;
using Avalonia.Interactivity;
using NetworkDriveManager.Models;

namespace NetworkDriveManager.Views;

/// <summary>
/// Help dialog displaying application usage information.
/// </summary>
public partial class HelpDialog : Window
{
    /// <summary>Localized help content shown in the text area.</summary>
    public string HelpContent { get; }
    /// <summary>Localized text for the close button.</summary>
    public string CloseText { get; }

    /// <summary>
    /// Initializes the help dialog with localized content for the given language.
    /// </summary>
    public HelpDialog(string lang)
    {
        HelpContent = Translations.Get(lang, "help_text");
        CloseText = Translations.Get(lang, "cancel");

        InitializeComponent();
        DataContext = this;

        Title = Translations.Get(lang, "help_title");
    }

    /// <summary>Required by Avalonia XAML loader.</summary>
    public HelpDialog() : this("en") { }

    /// <summary>Closes the help dialog.</summary>
    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
