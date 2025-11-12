using System.Runtime.InteropServices;
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
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);
    private const string TrayLogPath = "tray_log.txt";
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
    private HotkeyService? _hotkeyService;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LogTray("Application startup initiated");

        try
        {
            // Initialize services
            _configService = new ConfigService();
            LogTray("ConfigService initialized");
            var config = _configService.GetConfig();
            LogTray("Configuration loaded");

            LogTray("Initializing PerformanceMonitor");
            _performanceMonitor = new PerformanceMonitor();
            LogTray("PerformanceMonitor initialized");

            LogTray("Initializing ClipboardHistoryService");
            _clipboardHistoryService = new ClipboardHistoryService();
            LogTray("ClipboardHistoryService initialized");

            LogTray("Initializing SnippetService");
            _snippetService = new SnippetService();
            LogTray("SnippetService initialized");

            LogTray("Initializing PluginService");
            _pluginService = new PluginService("Data/plugins");
            LogTray("PluginService initialized");

            LogTray("Initializing VariableProcessor");
            _variableProcessor = new VariableProcessor(_clipboardHistoryService, _pluginService);
            LogTray("VariableProcessor initialized");

            LogTray("Initializing AppFilterService");
            _appFilterService = new AppFilterService(config);
            LogTray("AppFilterService initialized");

            LogTray("Initializing FormDialogService");
            _formDialogService = new FormDialogService();
            LogTray("FormDialogService initialized");

            LogTray("Initializing BackupService");
            _backupService = new BackupService("Data", _snippetService, _configService);
            LogTray("BackupService initialized");

            LogTray("Initializing ThemeService");
            _themeService = new ThemeService(_configService);
            LogTray("ThemeService initialized");

            LogTray("Initializing HotkeyService");
            _hotkeyService = new HotkeyService(_snippetService);
            LogTray("HotkeyService initialized");

            LogTray("Core services initialized");

            // Initialize keyboard hook
            _keyboardHookService = new KeyboardHookService(
                _snippetService,
                _variableProcessor,
                _appFilterService,
                _performanceMonitor,
                config,
                _formDialogService,
                _hotkeyService);

            _keyboardHookService.SnippetExpanded += (s, args) =>
            {
                // Handle snippet expansion event if needed
            };

            _keyboardHookService.Start();
            LogTray("Keyboard hook started");

            // Create and show main window first
            _mainWindow = new MainWindow(
                _snippetService,
                _variableProcessor,
                _formDialogService,
                _backupService);
            LogTray("Main window instantiated");

            if (!config.StartMinimized)
            {
                _mainWindow.Show();
                LogTray("Main window shown normally");
            }
            else
            {
                if (config.MinimizeToTray)
                {
                    _mainWindow.WindowState = WindowState.Minimized;
                    _mainWindow.Hide();
                    LogTray("Main window started minimized to tray");
                }
                else
                {
                    _mainWindow.WindowState = WindowState.Minimized;
                    LogTray("Main window started minimized (no tray)");
                }
            }

            // Handle window closing
            _mainWindow.Closing += (s, args) =>
            {
                if (config.MinimizeToTray)
                {
                    args.Cancel = true;
                    _mainWindow.Hide();
                    LogTray("Main window close intercepted; hiding instead");
                }
                else
                {
                    LogTray("Main window closing normally");
                }
            };

            LogTray("Main window initialized, setting up system tray");
            SetupSystemTray();
        }
        catch (Exception ex)
        {
            // Write error to file
            try
            {
                System.IO.File.WriteAllText("error_log.txt",
                    $"Fehler beim Starten der Anwendung:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            }
            catch { }
            LogTray($"Startup failed: {ex.Message}");

            System.Windows.MessageBox.Show(
                $"Fehler beim Starten der Anwendung:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Startfehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Try to show window anyway
            try
            {
                if (_mainWindow != null)
                {
                    _mainWindow.Show();
                }
                else
                {
                    // Create a simple fallback window
                    var fallbackWindow = new Window
                    {
                        Title = "XP Hotkey - Fehler",
                        Width = 400,
                        Height = 200,
                        Content = new System.Windows.Controls.TextBlock
                        {
                            Text = $"Die Anwendung konnte nicht vollständig gestartet werden.\n\nFehler: {ex.Message}",
                            Margin = new Thickness(20),
                            TextWrapping = TextWrapping.Wrap
                        }
                    };
                    fallbackWindow.Show();
                }
            }
            catch
            {
                // Last resort - just shut down
                Shutdown();
            }
        }
    }

    private void SetupSystemTray()
    {
        try
        {
            LogTray("Setting up system tray...");
            System.Diagnostics.Debug.WriteLine("Setting up system tray...");

            var icon = CreateTrayIcon();
            System.Diagnostics.Debug.WriteLine($"Icon created: {icon != null}");
            LogTray($"Icon created: {icon != null}");

            _notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Text = "XP Hotkey - Text Expander",
                Visible = false  // Set to false first
            };

            System.Diagnostics.Debug.WriteLine($"NotifyIcon instantiated");
            LogTray("NotifyIcon instantiated");

            _notifyIcon.DoubleClick += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("NotifyIcon double-clicked");
                if (_mainWindow != null)
                {
                    _mainWindow.Show();
                    _mainWindow.WindowState = WindowState.Normal;
                    _mainWindow.Activate();
                }
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Fenster oeffnen", null, (s, e) =>
            {
                if (_mainWindow != null)
                {
                    _mainWindow.Show();
                    _mainWindow.WindowState = WindowState.Normal;
                    _mainWindow.Activate();
                }
            });
            contextMenu.Items.Add("Snippet hinzufuegen (Ctrl+Shift+Q)", null, (s, e) =>
            {
                if (_mainWindow != null)
                {
                    _mainWindow.Show();
                    _mainWindow.WindowState = WindowState.Normal;
                    _mainWindow.Activate();
                    _mainWindow.ShowQuickAddDialog();
                }
            });
            contextMenu.Items.Add(new ToolStripSeparator());
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
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Beenden", null, (s, e) => Shutdown());

            _notifyIcon.ContextMenuStrip = contextMenu;

            // Now make it visible
            _notifyIcon.Visible = true;

            System.Diagnostics.Debug.WriteLine($"NotifyIcon visible set to: {_notifyIcon.Visible}");
            System.Diagnostics.Debug.WriteLine("System tray setup completed successfully");
            LogTray($"NotifyIcon visible set to: {_notifyIcon.Visible}");

            // Show balloon tip on startup
            try
            {
                _notifyIcon.ShowBalloonTip(5000, "XP Hotkey gestartet",
                    "Text Expander laeuft. Rechtsklick auf Icon fuer Optionen.",
                    ToolTipIcon.Info);
                System.Diagnostics.Debug.WriteLine("Balloon tip shown");
                LogTray("Balloon tip shown");
            }
            catch (Exception balloonEx)
            {
                System.Diagnostics.Debug.WriteLine($"Could not show balloon tip: {balloonEx.Message}");
                LogTray($"Balloon tip failed: {balloonEx.Message}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up system tray: {ex.Message}\n{ex.StackTrace}");
            System.Windows.MessageBox.Show(
                $"Fehler beim Einrichten des System Tray Icons:\n{ex.Message}\n\n{ex.StackTrace}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            LogTray($"System tray setup error: {ex.Message}");
        }
    }

    private System.Drawing.Icon CreateTrayIcon()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Creating tray icon...");

            // Create a 16x16 bitmap (standard tray icon size)
        using (var bitmap = new System.Drawing.Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        {
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Draw a colored background circle
                using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 0, 120, 215)))
                {
                    graphics.FillEllipse(brush, 0, 0, 16, 16);
                }

                // Draw "XP" text
                using (var font = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Bold))
                using (var textBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    var format = new System.Drawing.StringFormat
                    {
                        Alignment = System.Drawing.StringAlignment.Center,
                        LineAlignment = System.Drawing.StringAlignment.Center
                    };
                    graphics.DrawString("XP", font, textBrush, new System.Drawing.RectangleF(0, 0, 16, 16), format);
                }
            }

            // Convert bitmap to icon handle and clone it so we can clean up GDI resources
            var hIcon = bitmap.GetHicon();
            try
            {
                using var tempIcon = System.Drawing.Icon.FromHandle(hIcon);
                var clonedIcon = (System.Drawing.Icon)tempIcon.Clone();
                System.Diagnostics.Debug.WriteLine("Tray icon created successfully via HICON clone");
                LogTray("Tray icon created successfully via HICON clone");
                return clonedIcon;
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating tray icon: {ex.Message}\n{ex.StackTrace}");
            LogTray($"Error creating tray icon: {ex.Message}");
            System.Windows.MessageBox.Show($"Icon Fehler: {ex.Message}\n\n{ex.StackTrace}", "Debug");
            // Fallback: use a system icon
            return System.Drawing.SystemIcons.Application;
        }
    }

    private void LogTray(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:O}] {message}{Environment.NewLine}";
            System.IO.File.AppendAllText(TrayLogPath, line);
        }
        catch
        {
            // Ignore logging failures
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LogTray("Application exiting");
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
