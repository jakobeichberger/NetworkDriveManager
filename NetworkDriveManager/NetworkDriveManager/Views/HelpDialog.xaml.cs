using System.Windows;
using NetworkDriveManager.Models;

namespace NetworkDriveManager.Views;

/// <summary>
/// Help dialog displaying application usage information.
/// </summary>
public partial class HelpDialog : Window
{
    public new string Title { get; }
    public string HelpContent { get; }
    public string CloseText { get; }

    public HelpDialog(string lang)
    {
        Title = Translations.Get(lang, "help_title");
        HelpContent = Translations.Get(lang, "help_text");
        CloseText = Translations.Get(lang, "cancel");

        InitializeComponent();
        DataContext = this;
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
