namespace XP_Hotkey.Models;

public class BackupSettings
{
    public bool AutoBackupEnabled { get; set; } = true;
    public int BackupIntervalDays { get; set; } = 1;
    public int MaxBackups { get; set; } = 30;
    public string BackupLocation { get; set; } = "Data/backups";
}

