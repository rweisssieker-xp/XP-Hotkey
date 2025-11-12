using System.Text.RegularExpressions;
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
            UseRegexCheckBox.IsChecked = snippet.UseRegex;
            CaseSensitiveCheckBox.IsChecked = snippet.CaseSensitive;
            EnabledCheckBox.IsChecked = snippet.Enabled;
            HotkeyTextBox.Text = snippet.Hotkey ?? string.Empty;

            // Load app lists
            if (snippet.AllowedApps != null)
            {
                foreach (var app in snippet.AllowedApps)
                {
                    AllowedAppsListBox.Items.Add(app);
                }
            }
            if (snippet.BlockedApps != null)
            {
                foreach (var app in snippet.BlockedApps)
                {
                    BlockedAppsListBox.Items.Add(app);
                }
            }
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ShortcutTextBox.Text))
        {
            MessageBox.Show("Bitte geben Sie ein Kürzel ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var shortcut = ShortcutTextBox.Text.Trim();
        var useRegex = UseRegexCheckBox.IsChecked == true;

        // Validate regex pattern if UseRegex is enabled
        if (useRegex)
        {
            try
            {
                // Test if the pattern is valid
                _ = new Regex(shortcut);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Ungültiger regulärer Ausdruck:\n{ex.Message}",
                    "Regex-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        // Collect allowed and blocked apps from ListBoxes
        var allowedApps = new List<string>();
        foreach (var item in AllowedAppsListBox.Items)
        {
            if (item is string app && !string.IsNullOrWhiteSpace(app))
            {
                allowedApps.Add(app);
            }
        }

        var blockedApps = new List<string>();
        foreach (var item in BlockedAppsListBox.Items)
        {
            if (item is string app && !string.IsNullOrWhiteSpace(app))
            {
                blockedApps.Add(app);
            }
        }

        Result = new Snippet
        {
            Shortcut = shortcut,
            Text = TextTextBox.Text,
            Description = DescriptionTextBox.Text.Trim(),
            UseRegex = useRegex,
            CaseSensitive = CaseSensitiveCheckBox.IsChecked == true,
            Enabled = EnabledCheckBox.IsChecked == true,
            Hotkey = string.IsNullOrWhiteSpace(HotkeyTextBox.Text) ? null : HotkeyTextBox.Text.Trim(),
            AllowedApps = allowedApps,
            BlockedApps = blockedApps
        };

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void AddAllowedApp_Click(object sender, RoutedEventArgs e)
    {
        var appName = PromptForInput("App-Name eingeben (z.B. 'chrome', 'notepad', 'code'):", "App hinzufügen");
        if (!string.IsNullOrWhiteSpace(appName))
        {
            AllowedAppsListBox.Items.Add(appName.Trim());
        }
    }

    private void RemoveAllowedApp_Click(object sender, RoutedEventArgs e)
    {
        if (AllowedAppsListBox.SelectedItem != null)
        {
            AllowedAppsListBox.Items.Remove(AllowedAppsListBox.SelectedItem);
        }
    }

    private void AddBlockedApp_Click(object sender, RoutedEventArgs e)
    {
        var appName = PromptForInput("App-Name eingeben (z.B. 'chrome', 'notepad', 'code'):", "App hinzufügen");
        if (!string.IsNullOrWhiteSpace(appName))
        {
            BlockedAppsListBox.Items.Add(appName.Trim());
        }
    }

    private void RemoveBlockedApp_Click(object sender, RoutedEventArgs e)
    {
        if (BlockedAppsListBox.SelectedItem != null)
        {
            BlockedAppsListBox.Items.Remove(BlockedAppsListBox.SelectedItem);
        }
    }

    private string? PromptForInput(string message, string title)
    {
        var inputWindow = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var grid = new System.Windows.Controls.Grid();
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

        var textBlock = new System.Windows.Controls.TextBlock
        {
            Text = message,
            Margin = new Thickness(10),
            TextWrapping = TextWrapping.Wrap
        };
        System.Windows.Controls.Grid.SetRow(textBlock, 0);

        var textBox = new System.Windows.Controls.TextBox
        {
            Margin = new Thickness(10, 0, 10, 10)
        };
        System.Windows.Controls.Grid.SetRow(textBox, 0);
        textBlock.Margin = new Thickness(10, 10, 10, 5);

        var stackPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };
        System.Windows.Controls.Grid.SetRow(stackPanel, 1);

        var okButton = new System.Windows.Controls.Button
        {
            Content = "OK",
            Width = 75,
            Margin = new Thickness(5, 0, 0, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => { inputWindow.DialogResult = true; inputWindow.Close(); };

        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Abbrechen",
            Width = 75,
            Margin = new Thickness(5, 0, 0, 0),
            IsCancel = true
        };
        cancelButton.Click += (s, e) => { inputWindow.DialogResult = false; inputWindow.Close(); };

        stackPanel.Children.Add(okButton);
        stackPanel.Children.Add(cancelButton);

        var mainPanel = new System.Windows.Controls.StackPanel();
        mainPanel.Children.Add(textBlock);
        mainPanel.Children.Add(textBox);
        mainPanel.Children.Add(stackPanel);

        inputWindow.Content = mainPanel;
        textBox.Focus();

        return inputWindow.ShowDialog() == true ? textBox.Text : null;
    }
}

