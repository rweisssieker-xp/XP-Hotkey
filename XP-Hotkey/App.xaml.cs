using System.Windows;
using System.Windows.Forms;
using XP_Hotkey.Models;
using XP_Hotkey.Services;
using XP_Hotkey.Utilities;
using XP_Hotkey.Views;
using Application = System.Windows.Application;

namespace XP_Hotkey;

public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private KeyboardHookService? _keyboardHookService;
    private SnippetService? _snippetService;
    private VariableProcessor? _variableProcessor;
    private ClipboardHistoryService? _clipboardHistoryService;
    private ConfigService? _configService;
    private AppFilterService? _appFilterService;
    private PerformanceMonitor? _performanceMonitor;
    private FormDialogService? _formDialogService;
    private BackupService? _backupService;
    private ThemeService? _themeService;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize services
        _configService = new ConfigService();
        var config = _configService.GetConfig();

        _performanceMonitor = new PerformanceMonitor();
        _clipboardHistoryService = new ClipboardHistoryService();
        _snippetService = new SnippetService();
        _variableProcessor = new VariableProcessor(_clipboardHistoryService);
        _appFilterService = new AppFilterService(config);
        _formDialogService = new FormDialogService();
        _backupService = new BackupService("Data", _snippetService, _configService);
        _themeService = new ThemeService(_configService);

        // Initialize keyboard hook
        _keyboardHookService = new KeyboardHookService(
            _snippetService,
            _variableProcessor,
            _appFilterService,
            _performanceMonitor,
            config,
            _formDialogService);

        _keyboardHookService.SnippetExpanded += (s, args) =>
        {
            // Handle snippet expansion event if needed
        };

        _keyboardHookService.Start();

        // Setup system tray
        SetupSystemTray();

        // Create and show main window
        _mainWindow = new MainWindow(
            _snippetService,
            _variableProcessor,
            _formDialogService,
            _backupService);

        if (!config.StartMinimized)
        {
            _mainWindow.Show();
        }
        else
        {
            if (config.MinimizeToTray)
            {
                _mainWindow.WindowState = WindowState.Minimized;
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.WindowState = WindowState.Minimized;
            }
        }

        // Handle window closing
        _mainWindow.Closing += (s, args) =>
        {
            if (config.MinimizeToTray)
            {
                args.Cancel = true;
                _mainWindow.Hide();
            }
        };
    }

    private void SetupSystemTray()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "XP Hotkey - Text Expander",
            Visible = true
        };

        _notifyIcon.DoubleClick += (s, e) =>
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Fenster öffnen", null, (s, e) =>
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        });
        contextMenu.Items.Add("Einstellungen", null, (s, e) =>
        {
            if (_configService != null)
            {
                var settingsWindow = new SettingsWindow(_configService);
                settingsWindow.ShowDialog();
                // Update keyboard hook with new config
                if (_keyboardHookService != null)
                {
                    _keyboardHookService.UpdateConfig(_configService.GetConfig());
                }
            }
        });
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Beenden", null, (s, e) => Shutdown());

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _keyboardHookService?.Stop();
        _keyboardHookService?.Dispose();
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}
