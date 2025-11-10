using System.Windows;
using System.Windows.Input;
using XP_Hotkey.Models;
using MessageBox = System.Windows.MessageBox;

namespace XP_Hotkey.Views;

public partial class QuickAddDialog : Window
{
    public Snippet? Result { get; private set; }

    public QuickAddDialog()
    {
        InitializeComponent();
        ShortcutTextBox.Focus();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ShortcutTextBox.Text))
        {
            MessageBox.Show("Bitte geben Sie ein Kürzel ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = new Snippet
        {
            Shortcut = ShortcutTextBox.Text.Trim(),
            Text = TextTextBox.Text
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ShortcutTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            TextTextBox.Focus();
            e.Handled = true;
        }
    }

    private void TextTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
        {
            AddButton_Click(sender, e);
            e.Handled = true;
        }
    }
}

