using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XP_Hotkey.Models;
using XP_Hotkey.Utilities;

namespace XP_Hotkey.Services;

public class VariableProcessor
{
    private readonly ClipboardHistoryService? _clipboardHistoryService;
    private readonly PluginService? _pluginService;
    private readonly Dictionary<string, int> _counters = new();
    private readonly Dictionary<string, int> _namedCounters = new();
    private readonly Random _random = new();

    public VariableProcessor(ClipboardHistoryService? clipboardHistoryService = null, PluginService? pluginService = null)
    {
        _clipboardHistoryService = clipboardHistoryService;
        _pluginService = pluginService;
    }

    public string ProcessVariables(string text, Snippet? snippet = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = text;

        // Process all variable patterns
        result = ProcessDateVariables(result);
        result = ProcessTimeVariables(result);
        result = ProcessDateTimeVariables(result);
        result = ProcessUsernameVariables(result);
        result = ProcessClipboardVariables(result);
        result = ProcessRandomVariables(result);
        result = ProcessUuidVariables(result);
        result = ProcessCountVariables(result, snippet);
        result = ProcessConditionalVariables(result);
        result = ProcessRepeatVariables(result);
        result = ProcessPluginVariables(result);
        // Note: {cursor} is handled specially in KeyboardHookService during text sending

        return result;
    }

    private string ProcessDateVariables(string text)
    {
        // {date} or {date:format}
        return Regex.Replace(text, @"\{date(?::([^}]+))?\}", match =>
        {
            var format = match.Groups[1].Success ? match.Groups[1].Value : "dd.MM.yyyy";
            return DateTime.Now.ToString(format);
        });
    }

    private string ProcessTimeVariables(string text)
    {
        // {time} or {time:format}
        return Regex.Replace(text, @"\{time(?::([^}]+))?\}", match =>
        {
            var format = match.Groups[1].Success ? match.Groups[1].Value : "HH:mm";
            return DateTime.Now.ToString(format);
        });
    }

    private string ProcessDateTimeVariables(string text)
    {
        // {datetime} or {datetime:format}
        return Regex.Replace(text, @"\{datetime(?::([^}]+))?\}", match =>
        {
            var format = match.Groups[1].Success ? match.Groups[1].Value : "dd.MM.yyyy HH:mm";
            return DateTime.Now.ToString(format);
        });
    }

    private string ProcessUsernameVariables(string text)
    {
        return text.Replace("{username}", Environment.UserName);
    }

    private string ProcessClipboardVariables(string text)
    {
        // {clipboard}
        if (text.Contains("{clipboard}"))
        {
            try
            {
                var clipboardText = Clipboard.GetText();
                text = text.Replace("{clipboard}", clipboardText);
            }
            catch
            {
                text = text.Replace("{clipboard}", string.Empty);
            }
        }

        // {clipboard_history:N}
        return Regex.Replace(text, @"\{clipboard_history:(\d+)\}", match =>
        {
            if (_clipboardHistoryService == null)
                return string.Empty;

            var index = int.Parse(match.Groups[1].Value);
            var history = _clipboardHistoryService.GetHistory();
            if (index > 0 && index <= history.Count)
            {
                return history[index - 1];
            }
            return string.Empty;
        });
    }

    private string ProcessRandomVariables(string text)
    {
        // {random} - 0-100
        text = Regex.Replace(text, @"\{random\}", m => _random.Next(0, 101).ToString());

        // {random:min-max}
        return Regex.Replace(text, @"\{random:(\d+)-(\d+)\}", match =>
        {
            var min = int.Parse(match.Groups[1].Value);
            var max = int.Parse(match.Groups[2].Value);
            return _random.Next(min, max + 1).ToString();
        });
    }

    private string ProcessUuidVariables(string text)
    {
        return Regex.Replace(text, @"\{uuid\}", m => Guid.NewGuid().ToString());
    }

    private string ProcessCountVariables(string text, Snippet? snippet)
    {
        // {count} - per snippet
        if (snippet != null && text.Contains("{count}"))
        {
            if (!_counters.ContainsKey(snippet.Id))
            {
                _counters[snippet.Id] = 0;
            }
            _counters[snippet.Id]++;
            text = text.Replace("{count}", _counters[snippet.Id].ToString());
        }

        // {count:name} - named counter
        return Regex.Replace(text, @"\{count:([^}]+)\}", match =>
        {
            var name = match.Groups[1].Value;
            if (!_namedCounters.ContainsKey(name))
            {
                _namedCounters[name] = 0;
            }
            _namedCounters[name]++;
            return _namedCounters[name].ToString();
        });
    }

    private string ProcessConditionalVariables(string text)
    {
        // {if:condition:true:false}
        return Regex.Replace(text, @"\{if:([^:]+):([^:]+):([^}]+)\}", match =>
        {
            var condition = match.Groups[1].Value.Trim();
            var trueValue = match.Groups[2].Value;
            var falseValue = match.Groups[3].Value;

            // Simple condition evaluation (can be extended)
            bool result = !string.IsNullOrWhiteSpace(condition) && 
                         condition.ToLower() != "false" && 
                         condition != "0";

            return result ? trueValue : falseValue;
        });
    }

    private string ProcessRepeatVariables(string text)
    {
        // {repeat:text:count}
        return Regex.Replace(text, @"\{repeat:([^:]+):(\d+)\}", match =>
        {
            var repeatText = match.Groups[1].Value;
            var count = int.Parse(match.Groups[2].Value);
            return string.Join("", Enumerable.Repeat(repeatText, count));
        });
    }

    private string ProcessPluginVariables(string text)
    {
        if (_pluginService == null)
            return text;

        // Find all {variable} patterns that weren't handled by built-in processors
        // This regex matches {variable} or {variable:param} patterns
        return Regex.Replace(text, @"\{([^}:]+)(?::([^}]+))?\}", match =>
        {
            var variableName = match.Groups[1].Value;
            var paramValue = match.Groups[2].Success ? match.Groups[2].Value : null;

            // Skip built-in variables that should have been processed already
            var builtInVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "date", "time", "datetime", "username", "clipboard", "clipboard_history",
                "random", "uuid", "count", "if", "repeat", "cursor"
            };

            if (builtInVariables.Contains(variableName))
            {
                // Return original if it's a built-in variable (shouldn't happen, but safe)
                return match.Value;
            }

            // Prepare parameters dictionary
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(paramValue))
            {
                parameters["param"] = paramValue;
            }

            // Try to process via plugin service
            var result = _pluginService.ProcessVariable(variableName, parameters);
            if (result != null)
            {
                return result;
            }

            // If no plugin handled it, return original
            return match.Value;
        });
    }

    public void ResetCounter(string snippetId)
    {
        _counters.Remove(snippetId);
    }

    public void ResetNamedCounter(string name)
    {
        _namedCounters.Remove(name);
    }

    public void ResetAllCounters()
    {
        _counters.Clear();
        _namedCounters.Clear();
    }
}

