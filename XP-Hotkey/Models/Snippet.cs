using System.Collections.Generic;

namespace XP_Hotkey.Models;

public class Snippet
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Shortcut { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsFavorite { get; set; }
    public bool CaseSensitive { get; set; }
    public bool UseRegex { get; set; }
    public List<FormField> FormFields { get; set; } = new();
    public SnippetStatistics Statistics { get; set; } = new();
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Modified { get; set; } = DateTime.Now;
    public DateTime LastUsed { get; set; } = DateTime.MinValue;
    public string? Hotkey { get; set; } // Format: "Ctrl+Shift+K"
    public bool Enabled { get; set; } = true;
}

