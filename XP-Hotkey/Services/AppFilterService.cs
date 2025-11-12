using System.Diagnostics;
using System.Runtime.InteropServices;
using XP_Hotkey.Models;

namespace XP_Hotkey.Services;

public class AppFilterService
{
    private readonly AppConfig _config;

    public AppFilterService(AppConfig config)
    {
        _config = config;
    }

    public bool IsAllowed()
    {
        var currentProcess = GetForegroundProcess();
        if (currentProcess == null)
            return true;

        var processName = currentProcess.ProcessName.ToLower();
        var executablePath = currentProcess.MainModule?.FileName?.ToLower() ?? string.Empty;

        // Check blacklist first
        if (_config.AppBlacklist != null && _config.AppBlacklist.Count > 0)
        {
            foreach (var blacklisted in _config.AppBlacklist)
            {
                var blacklistedLower = blacklisted.ToLower();
                if (processName.Contains(blacklistedLower) ||
                    executablePath.Contains(blacklistedLower))
                {
                    return false;
                }
            }
        }

        // Check whitelist
        if (_config.AppWhitelist != null && _config.AppWhitelist.Count > 0)
        {
            foreach (var whitelisted in _config.AppWhitelist)
            {
                var whitelistedLower = whitelisted.ToLower();
                if (processName.Contains(whitelistedLower) ||
                    executablePath.Contains(whitelistedLower))
                {
                    return true;
                }
            }
            // If whitelist exists but no match, deny
            return false;
        }

        // No filters, allow all
        return true;
    }

    /// <summary>
    /// Checks if a specific snippet is allowed in the current application
    /// </summary>
    /// <param name="snippet">The snippet to check</param>
    /// <returns>True if the snippet should be allowed, false otherwise</returns>
    public bool IsSnippetAllowedInCurrentApp(Models.Snippet snippet)
    {
        var currentProcess = GetForegroundProcess();
        if (currentProcess == null)
            return true;

        var processName = currentProcess.ProcessName.ToLower();
        var executablePath = currentProcess.MainModule?.FileName?.ToLower() ?? string.Empty;

        // Check snippet-specific blocked apps first
        if (snippet.BlockedApps != null && snippet.BlockedApps.Count > 0)
        {
            foreach (var blocked in snippet.BlockedApps)
            {
                var blockedLower = blocked.ToLower();
                if (processName.Contains(blockedLower) ||
                    executablePath.Contains(blockedLower))
                {
                    return false;
                }
            }
        }

        // Check snippet-specific allowed apps
        if (snippet.AllowedApps != null && snippet.AllowedApps.Count > 0)
        {
            foreach (var allowed in snippet.AllowedApps)
            {
                var allowedLower = allowed.ToLower();
                if (processName.Contains(allowedLower) ||
                    executablePath.Contains(allowedLower))
                {
                    return true;
                }
            }
            // If allowed apps list exists but no match, deny
            return false;
        }

        // No snippet-specific filters, allow
        return true;
    }

    /// <summary>
    /// Gets the name of the current foreground process (for UI display)
    /// </summary>
    public string? GetCurrentProcessName()
    {
        var process = GetForegroundProcess();
        return process?.ProcessName;
    }

    private Process? GetForegroundProcess()
    {
        try
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return null;

            GetWindowThreadProcessId(hwnd, out uint processId);
            return Process.GetProcessById((int)processId);
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}

