namespace XP_Hotkey.Models;

public class TriggerSettings
{
    public bool UseSpace { get; set; } = true;
    public bool UseTab { get; set; } = true;
    public bool UseEnter { get; set; } = false;
    public int MaxBufferSize { get; set; } = 50; // Maximale Zeichen für Buffer
}

