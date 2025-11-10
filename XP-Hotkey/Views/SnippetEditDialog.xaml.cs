using System.Windows;
using XP_Hotkey.Models;
using MessageBox = System.Windows.MessageBox;

namespace XP_Hotkey.Views;

public partial class SnippetEditDialog : Window
{
    public Snippet? Result { get; private set; }

    public SnippetEditDialog(Snippet? snippet = null)
    {
        InitializeComponent();

        if (snippet != null)
        {
            ShortcutTextBox.Text = snippet.Shortcut;
            TextTextBox.Text = snippet.Text;
            DescriptionTextBox.Text = snippet.Description ?? string.Empty;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ShortcutTextBox.Text))
        {
            MessageBox.Show("Bitte geben Sie ein Kürzel ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = new Snippet
        {
            Shortcut = ShortcutTextBox.Text.Trim(),
            Text = TextTextBox.Text,
            Description = DescriptionTextBox.Text.Trim()
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

