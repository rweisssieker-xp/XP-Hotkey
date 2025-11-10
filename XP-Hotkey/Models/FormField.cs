namespace XP_Hotkey.Models;

public class FormField
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text"; // text, number, email, date, etc.
    public bool Required { get; set; }
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? ValidationPattern { get; set; }
    public string? ValidationMessage { get; set; }
}

