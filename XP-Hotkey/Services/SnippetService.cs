using System.Collections.Generic;
using System.IO;
using System.Linq;
using XP_Hotkey.Models;
using XP_Hotkey.Utilities;

namespace XP_Hotkey.Services;

public class SnippetService
{
    private readonly string _dataPath;
    private List<Snippet> _snippets = new();
    private readonly object _lock = new();

    public SnippetService(string dataPath = "Data")
    {
        _dataPath = dataPath;
        EnsureDataDirectory();
        LoadSnippets();
    }

    private void EnsureDataDirectory()
    {
        if (!Directory.Exists(_dataPath))
        {
            Directory.CreateDirectory(_dataPath);
        }
    }

    public List<Snippet> GetAllSnippets()
    {
        lock (_lock)
        {
            return _snippets.ToList();
        }
    }

    public List<Snippet> GetEnabledSnippets()
    {
        lock (_lock)
        {
            return _snippets.Where(s => s.Enabled).ToList();
        }
    }

    public Snippet? GetSnippetById(string id)
    {
        lock (_lock)
        {
            return _snippets.FirstOrDefault(s => s.Id == id);
        }
    }

    public Snippet? GetSnippetByShortcut(string shortcut, bool caseSensitive = false)
    {
        lock (_lock)
        {
            if (caseSensitive)
            {
                return _snippets.FirstOrDefault(s => s.Shortcut == shortcut && s.Enabled);
            }
            return _snippets.FirstOrDefault(s => 
                s.Shortcut.Equals(shortcut, StringComparison.OrdinalIgnoreCase) && s.Enabled);
        }
    }

    public List<Snippet> SearchSnippets(string searchTerm)
    {
        lock (_lock)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _snippets.ToList();

            var term = searchTerm.ToLower();
            return _snippets.Where(s =>
                s.Shortcut.ToLower().Contains(term) ||
                s.Text.ToLower().Contains(term) ||
                (s.Description != null && s.Description.ToLower().Contains(term)) ||
                s.Tags.Any(t => t.ToLower().Contains(term))
            ).ToList();
        }
    }

    public List<Snippet> GetSnippetsByCategory(string category)
    {
        lock (_lock)
        {
            return _snippets.Where(s => s.Categories.Contains(category)).ToList();
        }
    }

    public List<Snippet> GetFavoriteSnippets()
    {
        lock (_lock)
        {
            return _snippets.Where(s => s.IsFavorite).ToList();
        }
    }

    public void AddSnippet(Snippet snippet)
    {
        lock (_lock)
        {
            // Check for duplicate shortcut
            if (_snippets.Any(s => s.Shortcut.Equals(snippet.Shortcut, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Ein Snippet mit dem Kürzel '{snippet.Shortcut}' existiert bereits.");
            }
            
            if (string.IsNullOrEmpty(snippet.Id))
            {
                snippet.Id = Guid.NewGuid().ToString();
            }
            snippet.Created = DateTime.Now;
            snippet.Modified = DateTime.Now;
            _snippets.Add(snippet);
            SaveSnippets();
        }
    }

    public void UpdateSnippet(Snippet snippet)
    {
        lock (_lock)
        {
            var existing = _snippets.FirstOrDefault(s => s.Id == snippet.Id);
            if (existing != null)
            {
                // Check for duplicate shortcut (excluding current snippet)
                if (_snippets.Any(s => s.Id != snippet.Id && 
                    s.Shortcut.Equals(snippet.Shortcut, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Ein Snippet mit dem Kürzel '{snippet.Shortcut}' existiert bereits.");
                }
                
                snippet.Modified = DateTime.Now;
                var index = _snippets.IndexOf(existing);
                _snippets[index] = snippet;
                SaveSnippets();
            }
        }
    }

    public void DeleteSnippet(string id)
    {
        lock (_lock)
        {
            var snippet = _snippets.FirstOrDefault(s => s.Id == id);
            if (snippet != null)
            {
                _snippets.Remove(snippet);
                SaveSnippets();
            }
        }
    }

    public void DuplicateSnippet(string id)
    {
        lock (_lock)
        {
            var snippet = _snippets.FirstOrDefault(s => s.Id == id);
            if (snippet != null)
            {
                var duplicate = new Snippet
                {
                    Shortcut = snippet.Shortcut + "_copy",
                    Text = snippet.Text,
                    Description = snippet.Description,
                    Categories = new List<string>(snippet.Categories),
                    Tags = new List<string>(snippet.Tags),
                    IsFavorite = snippet.IsFavorite,
                    CaseSensitive = snippet.CaseSensitive,
                    UseRegex = snippet.UseRegex,
                    FormFields = snippet.FormFields.Select(f => new FormField
                    {
                        Name = f.Name,
                        Label = f.Label,
                        Type = f.Type,
                        Required = f.Required,
                        DefaultValue = f.DefaultValue,
                        Placeholder = f.Placeholder,
                        ValidationPattern = f.ValidationPattern,
                        ValidationMessage = f.ValidationMessage
                    }).ToList(),
                    Enabled = snippet.Enabled
                };
                AddSnippet(duplicate);
            }
        }
    }

    public void RecordUsage(string snippetId)
    {
        lock (_lock)
        {
            var snippet = _snippets.FirstOrDefault(s => s.Id == snippetId);
            if (snippet != null)
            {
                snippet.LastUsed = DateTime.Now;
                if (snippet.Statistics.FirstUsed == DateTime.MinValue)
                {
                    snippet.Statistics.FirstUsed = DateTime.Now;
                }
                snippet.Statistics.LastUsed = DateTime.Now;
                snippet.Statistics.UseCount++;
                SaveSnippets();
            }
        }
    }

    public void LoadSnippets()
    {
        LoadSnippetsAsync().GetAwaiter().GetResult();
    }

    private async Task LoadSnippetsAsync()
    {
        var filePath = Path.Combine(_dataPath, "snippets.json");
        if (File.Exists(filePath))
        {
            try
            {
                var snippets = await JsonHelper.DeserializeFromFileAsync<List<Snippet>>(filePath);
                if (snippets != null)
                {
                    lock (_lock)
                    {
                        _snippets = snippets;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error - in production use proper logging framework
                System.Diagnostics.Debug.WriteLine($"Error loading snippets: {ex.Message}");
                // Start with empty list on error
            }
        }
    }

    private void SaveSnippets()
    {
        SaveSnippetsAsync().GetAwaiter().GetResult();
    }

    private async Task SaveSnippetsAsync()
    {
        var filePath = Path.Combine(_dataPath, "snippets.json");
        try
        {
            List<Snippet> snippetsCopy;
            lock (_lock)
            {
                snippetsCopy = _snippets.ToList();
            }
            await JsonHelper.SerializeToFileAsync(snippetsCopy, filePath);
        }
        catch (Exception ex)
        {
            // Log error - in production use proper logging framework
            System.Diagnostics.Debug.WriteLine($"Error saving snippets: {ex.Message}");
            throw; // Re-throw to notify caller of save failure
        }
    }

    public void ExportToJson(string filePath)
    {
        ExportToJsonAsync(filePath).GetAwaiter().GetResult();
    }

    public async Task ExportToJsonAsync(string filePath)
    {
        List<Snippet> snippetsCopy;
        lock (_lock)
        {
            snippetsCopy = _snippets.ToList();
        }
        await JsonHelper.SerializeToFileAsync(snippetsCopy, filePath);
    }

    public void ImportFromJson(string filePath)
    {
        ImportFromJsonAsync(filePath).GetAwaiter().GetResult();
    }

    public async Task ImportFromJsonAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Import file not found", filePath);
        }

        try
        {
            var imported = await JsonHelper.DeserializeFromFileAsync<List<Snippet>>(filePath);
            if (imported != null)
            {
                lock (_lock)
                {
                    foreach (var snippet in imported)
                    {
                        // Generate new IDs for imported snippets to avoid conflicts
                        snippet.Id = Guid.NewGuid().ToString();
                        snippet.Created = DateTime.Now;
                        snippet.Modified = DateTime.Now;
                        _snippets.Add(snippet);
                    }
                }
                await SaveSnippetsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error importing snippets: {ex.Message}");
            throw new Exception($"Failed to import snippets from file: {ex.Message}", ex);
        }
    }
}

