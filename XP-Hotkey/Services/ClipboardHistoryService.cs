using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XP_Hotkey.Models;
using XP_Hotkey.Utilities;

namespace XP_Hotkey.Services;

public class ClipboardHistoryService
{
    private readonly List<string> _history = new();
    private readonly int _maxHistorySize;
    private readonly string _dataPath;
    private string? _lastClipboardText;

    public ClipboardHistoryService(int maxHistorySize = 50, string dataPath = "Data")
    {
        _maxHistorySize = maxHistorySize;
        _dataPath = dataPath;
        LoadHistory();
        StartMonitoring();
    }

    private void StartMonitoring()
    {
        var timer = new System.Windows.Forms.Timer();
        timer.Interval = 500; // Check every 500ms
        timer.Tick += (s, e) => CheckClipboard();
        timer.Start();
    }

    private void CheckClipboard()
    {
        try
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                if (!string.IsNullOrEmpty(text) && text != _lastClipboardText)
                {
                    AddToHistory(text);
                    _lastClipboardText = text;
                }
            }
        }
        catch
        {
            // Ignore clipboard access errors
        }
    }

    private void AddToHistory(string text)
    {
        // Remove if already exists (move to top)
        _history.Remove(text);
        _history.Insert(0, text);

        // Limit size
        while (_history.Count > _maxHistorySize)
        {
            _history.RemoveAt(_history.Count - 1);
        }

        SaveHistory();
    }

    public List<string> GetHistory(int count = 10)
    {
        return _history.Take(count).ToList();
    }

    public void ClearHistory()
    {
        _history.Clear();
        SaveHistory();
    }

    private void LoadHistory()
    {
        var filePath = Path.Combine(_dataPath, "clipboard_history.json");
        if (File.Exists(filePath))
        {
            try
            {
                var history = JsonHelper.DeserializeFromFileAsync<List<string>>(filePath).Result;
                if (history != null)
                {
                    _history.Clear();
                    _history.AddRange(history.Take(_maxHistorySize));
                }
            }
            catch
            {
                // Ignore load errors
            }
        }
    }

    private void SaveHistory()
    {
        var filePath = Path.Combine(_dataPath, "clipboard_history.json");
        try
        {
            JsonHelper.SerializeToFileAsync(_history, filePath).Wait();
        }
        catch
        {
            // Ignore save errors
        }
    }
}

