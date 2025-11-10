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
    private PluginService? _pluginService;
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
        _pluginService = new PluginService("Data/plugins");
        _variableProcessor = new VariableProcessor(_clipboardHistoryService, _pluginService);
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

        // Create and show main window first
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

        // Setup system tray after main window is created
        SetupSystemTray();
    }

    private void SetupSystemTray()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Setting up system tray...");
            
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateTrayIcon(),
                Text = "XP Hotkey - Text Expander",
                Visible = true,
                BalloonTipTitle = "XP Hotkey",
                BalloonTipText = "Text Expander läuft im Hintergrund"
            };

            System.Diagnostics.Debug.WriteLine($"NotifyIcon created. Visible: {_notifyIcon.Visible}");

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
            contextMenu.Items.Add("📝 Fenster öffnen", null, (s, e) =>
            {
                if (_mainWindow != null)
                {
                    _mainWindow.Show();
                    _mainWindow.WindowState = WindowState.Normal;
                    _mainWindow.Activate();
                }
            });
            contextMenu.Items.Add("⚙️ Einstellungen", null, (s, e) =>
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
            contextMenu.Items.Add("❌ Beenden", null, (s, e) => Shutdown());

            _notifyIcon.ContextMenuStrip = contextMenu;
            
            // Show balloon tip on startup
            _notifyIcon.ShowBalloonTip(2000);
            
            System.Diagnostics.Debug.WriteLine("System tray setup completed successfully");
            
            // Show a test message box to confirm the app is running
            System.Windows.MessageBox.Show(
                "XP Hotkey läuft jetzt im System Tray!\n\nSuche nach dem blauen 'XP' Icon in der Taskleiste.",
                "XP Hotkey gestartet",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up system tray: {ex.Message}\n{ex.StackTrace}");
            System.Windows.MessageBox.Show(
                $"Fehler beim Einrichten des System Tray Icons:\n{ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private System.Drawing.Icon CreateTrayIcon()
    {
        // Create a simple icon with text "XP"
        var bitmap = new System.Drawing.Bitmap(16, 16);
        using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
        {
            graphics.Clear(System.Drawing.Color.Transparent);
            
            // Draw a colored background
            using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 120, 215)))
            {
                graphics.FillRectangle(brush, 0, 0, 16, 16);
            }
            
            // Draw "XP" text
            using (var font = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Bold))
            using (var textBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {
                graphics.DrawString("XP", font, textBrush, -1, 2);
            }
        }
        
        return System.Drawing.Icon.FromHandle(bitmap.GetHicon());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Stop and dispose keyboard hook
        _keyboardHookService?.Stop();
        _keyboardHookService?.Dispose();
        
        // Dispose clipboard history service
        _clipboardHistoryService?.Dispose();
        
        // Dispose notify icon
        _notifyIcon?.Dispose();
        
        // Note: Other services don't implement IDisposable currently
        // but could be extended if they manage unmanaged resources
        
        base.OnExit(e);
    }
}
