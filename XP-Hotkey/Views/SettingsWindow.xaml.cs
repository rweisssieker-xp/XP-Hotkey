using System.Windows;
using XP_Hotkey.Models;
using XP_Hotkey.Services;

namespace XP_Hotkey.Views;

public partial class SettingsWindow : Window
{
    private readonly ConfigService _configService;

    public SettingsWindow(ConfigService configService)
    {
        InitializeComponent();
        _configService = configService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var config = _configService.GetConfig();

        UseSpaceCheckBox.IsChecked = config.Triggers.UseSpace;
        UseTabCheckBox.IsChecked = config.Triggers.UseTab;
        UseEnterCheckBox.IsChecked = config.Triggers.UseEnter;
        MaxBufferSizeTextBox.Text = config.Triggers.MaxBufferSize.ToString();

        StartMinimizedCheckBox.IsChecked = config.StartMinimized;
        MinimizeToTrayCheckBox.IsChecked = config.MinimizeToTray;

        DarkModeCheckBox.IsChecked = config.Theme.UseDarkMode;
        AccentColorTextBox.Text = config.Theme.AccentColor;

        AutoBackupCheckBox.IsChecked = config.Backup.AutoBackupEnabled;
        BackupIntervalTextBox.Text = config.Backup.BackupIntervalDays.ToString();
        MaxBackupsTextBox.Text = config.Backup.MaxBackups.ToString();

        WhitelistListBox.ItemsSource = config.AppWhitelist;
        BlacklistListBox.ItemsSource = config.AppBlacklist;
    }

    private void SaveSettings()
    {
        _configService.UpdateConfig(config =>
        {
            config.Triggers.UseSpace = UseSpaceCheckBox.IsChecked ?? false;
            config.Triggers.UseTab = UseTabCheckBox.IsChecked ?? false;
            config.Triggers.UseEnter = UseEnterCheckBox.IsChecked ?? false;
            if (int.TryParse(MaxBufferSizeTextBox.Text, out var bufferSize))
            {
                config.Triggers.MaxBufferSize = bufferSize;
            }

            config.StartMinimized = StartMinimizedCheckBox.IsChecked ?? false;
            config.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked ?? false;

            config.Theme.UseDarkMode = DarkModeCheckBox.IsChecked ?? false;
            config.Theme.AccentColor = AccentColorTextBox.Text;

            config.Backup.AutoBackupEnabled = AutoBackupCheckBox.IsChecked ?? false;
            if (int.TryParse(BackupIntervalTextBox.Text, out var interval))
            {
                config.Backup.BackupIntervalDays = interval;
            }
            if (int.TryParse(MaxBackupsTextBox.Text, out var maxBackups))
            {
                config.Backup.MaxBackups = maxBackups;
            }
        });
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        SaveSettings();
        base.OnClosing(e);
    }
}

