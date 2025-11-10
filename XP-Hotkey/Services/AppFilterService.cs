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

