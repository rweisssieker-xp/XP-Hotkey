namespace XP_Hotkey.Models;

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentId { get; set; }
    public string Color { get; set; } = "#0078D4";
    public int Order { get; set; }
}

