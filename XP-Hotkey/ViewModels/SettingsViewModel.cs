using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using XP_Hotkey.Models;
using XP_Hotkey.Services;

namespace XP_Hotkey.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ConfigService _configService;
    private AppConfig _config;

    private bool _useSpace = true;
    private bool _useTab = true;
    private bool _useEnter = false;
    private int _maxBufferSize = 50;
    private bool _startMinimized = false;
    private bool _minimizeToTray = true;
    private bool _useDarkMode = false;
    private string _accentColor = "#0078D4";
    private bool _autoBackupEnabled = true;
    private int _backupIntervalDays = 1;
    private int _maxBackups = 30;

    public SettingsViewModel(ConfigService configService)
    {
        _configService = configService;
        _config = configService.GetConfig();
        LoadSettings();

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public bool UseSpace
    {
        get => _useSpace;
        set
        {
            _useSpace = value;
            OnPropertyChanged();
        }
    }

    public bool UseTab
    {
        get => _useTab;
        set
        {
            _useTab = value;
            OnPropertyChanged();
        }
    }

    public bool UseEnter
    {
        get => _useEnter;
        set
        {
            _useEnter = value;
            OnPropertyChanged();
        }
    }

    public int MaxBufferSize
    {
        get => _maxBufferSize;
        set
        {
            _maxBufferSize = value;
            OnPropertyChanged();
        }
    }

    public bool StartMinimized
    {
        get => _startMinimized;
        set
        {
            _startMinimized = value;
            OnPropertyChanged();
        }
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set
        {
            _minimizeToTray = value;
            OnPropertyChanged();
        }
    }

    public bool UseDarkMode
    {
        get => _useDarkMode;
        set
        {
            _useDarkMode = value;
            OnPropertyChanged();
        }
    }

    public string AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = value;
            OnPropertyChanged();
        }
    }

    public bool AutoBackupEnabled
    {
        get => _autoBackupEnabled;
        set
        {
            _autoBackupEnabled = value;
            OnPropertyChanged();
        }
    }

    public int BackupIntervalDays
    {
        get => _backupIntervalDays;
        set
        {
            _backupIntervalDays = value;
            OnPropertyChanged();
        }
    }

    public int MaxBackups
    {
        get => _maxBackups;
        set
        {
            _maxBackups = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> AppWhitelist { get; } = new();
    public ObservableCollection<string> AppBlacklist { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public bool DialogResult { get; private set; }

    private void LoadSettings()
    {
        UseSpace = _config.Triggers.UseSpace;
        UseTab = _config.Triggers.UseTab;
        UseEnter = _config.Triggers.UseEnter;
        MaxBufferSize = _config.Triggers.MaxBufferSize;
        StartMinimized = _config.StartMinimized;
        MinimizeToTray = _config.MinimizeToTray;
        UseDarkMode = _config.Theme.UseDarkMode;
        AccentColor = _config.Theme.AccentColor;
        AutoBackupEnabled = _config.Backup.AutoBackupEnabled;
        BackupIntervalDays = _config.Backup.BackupIntervalDays;
        MaxBackups = _config.Backup.MaxBackups;

        AppWhitelist.Clear();
        foreach (var item in _config.AppWhitelist)
        {
            AppWhitelist.Add(item);
        }

        AppBlacklist.Clear();
        foreach (var item in _config.AppBlacklist)
        {
            AppBlacklist.Add(item);
        }
    }

    private void Save()
    {
        _configService.UpdateConfig(config =>
        {
            config.Triggers.UseSpace = UseSpace;
            config.Triggers.UseTab = UseTab;
            config.Triggers.UseEnter = UseEnter;
            config.Triggers.MaxBufferSize = MaxBufferSize;
            config.StartMinimized = StartMinimized;
            config.MinimizeToTray = MinimizeToTray;
            config.Theme.UseDarkMode = UseDarkMode;
            config.Theme.AccentColor = AccentColor;
            config.Backup.AutoBackupEnabled = AutoBackupEnabled;
            config.Backup.BackupIntervalDays = BackupIntervalDays;
            config.Backup.MaxBackups = MaxBackups;
            config.AppWhitelist = AppWhitelist.ToList();
            config.AppBlacklist = AppBlacklist.ToList();
        });

        DialogResult = true;
    }

    private void Cancel()
    {
        DialogResult = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

