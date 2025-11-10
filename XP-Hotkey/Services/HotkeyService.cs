using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using XP_Hotkey.Models;

namespace XP_Hotkey.Services;

public class HotkeyService
{
    private readonly SnippetService _snippetService;
    private readonly Dictionary<string, KeyCombination> _registeredHotkeys = new();
    private readonly KeyboardHookService _keyboardHook;

    public HotkeyService(SnippetService snippetService, KeyboardHookService keyboardHook)
    {
        _snippetService = snippetService;
        _keyboardHook = keyboardHook;
        LoadHotkeys();
    }

    public void RegisterHotkey(string snippetId, string hotkeyString)
    {
        if (TryParseHotkey(hotkeyString, out var combination))
        {
            _registeredHotkeys[snippetId] = combination;
            SaveHotkeys();
        }
    }

    public void UnregisterHotkey(string snippetId)
    {
        _registeredHotkeys.Remove(snippetId);
        SaveHotkeys();
    }

    public bool ProcessHotkey(Key key, bool ctrl, bool alt, bool shift, bool win)
    {
        var combination = new KeyCombination
        {
            Key = key,
            Ctrl = ctrl,
            Alt = alt,
            Shift = shift,
            Win = win
        };

        var hotkey = _registeredHotkeys.FirstOrDefault(h => h.Value.Equals(combination));
        if (hotkey.Key != null)
        {
            var snippet = _snippetService.GetSnippetById(hotkey.Key);
            if (snippet != null)
            {
                // Trigger snippet expansion
                // This would need to be integrated with KeyboardHookService
                return true;
            }
        }

        return false;
    }

    private bool TryParseHotkey(string hotkeyString, out KeyCombination combination)
    {
        combination = new KeyCombination();
        var parts = hotkeyString.Split('+').Select(p => p.Trim().ToLower()).ToList();

        foreach (var part in parts)
        {
            if (part == "ctrl")
                combination.Ctrl = true;
            else if (part == "alt")
                combination.Alt = true;
            else if (part == "shift")
                combination.Shift = true;
            else if (part == "win")
                combination.Win = true;
            else
            {
                // Try to parse as key
                if (Enum.TryParse<Key>(part, true, out var key))
                {
                    combination.Key = key;
                }
                else
                {
                    return false;
                }
            }
        }

        return combination.Key != Key.None;
    }

    private void LoadHotkeys()
    {
        var snippets = _snippetService.GetAllSnippets();
        foreach (var snippet in snippets)
        {
            if (!string.IsNullOrEmpty(snippet.Hotkey))
            {
                RegisterHotkey(snippet.Id, snippet.Hotkey);
            }
        }
    }

    private void SaveHotkeys()
    {
        // Hotkeys are saved as part of snippets
    }
}

public class KeyCombination
{
    public Key Key { get; set; }
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Win { get; set; }

    public bool Equals(KeyCombination other)
    {
        return Key == other.Key &&
               Ctrl == other.Ctrl &&
               Alt == other.Alt &&
               Shift == other.Shift &&
               Win == other.Win;
    }
}

