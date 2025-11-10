namespace XP_Hotkey.Models;

public class SnippetStatistics
{
    public int UseCount { get; set; }
    public DateTime FirstUsed { get; set; }
    public DateTime LastUsed { get; set; }
    public TimeSpan TotalTimeSaved { get; set; }

    public SnippetStatistics()
    {
        UseCount = 0;
        FirstUsed = DateTime.MinValue;
        LastUsed = DateTime.MinValue;
        TotalTimeSaved = TimeSpan.Zero;
    }
}

