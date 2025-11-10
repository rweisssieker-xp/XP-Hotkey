namespace XP_Hotkey.Models;

public class PerformanceSettings
{
    public bool EnablePerformanceMonitoring { get; set; } = true;
    public int MaxLatencyMs { get; set; } = 100;
    public bool LogSlowExpansions { get; set; } = false;
}

