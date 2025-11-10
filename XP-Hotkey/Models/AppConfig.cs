using System.Collections.Generic;

namespace XP_Hotkey.Models;

public class AppConfig
{
    public TriggerSettings Triggers { get; set; } = new();
    public bool StartMinimized { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public ThemeSettings Theme { get; set; } = new();
    public List<string> AppWhitelist { get; set; } = new();
    public List<string> AppBlacklist { get; set; } = new();
    public BackupSettings Backup { get; set; } = new();
    public bool EncryptSensitiveSnippets { get; set; } = false;
    public PerformanceSettings Performance { get; set; } = new();
    public string? MasterPasswordHash { get; set; }
    public bool ShowLivePreview { get; set; } = true;
    public bool EnableUndoRedo { get; set; } = true;
}

