namespace XP_Hotkey.Models;

public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public long Size { get; set; }
    public string? Description { get; set; }
    public int SnippetCount { get; set; }
}

