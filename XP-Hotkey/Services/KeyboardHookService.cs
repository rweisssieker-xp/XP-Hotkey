using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using XP_Hotkey.Models;
using XP_Hotkey.Utilities;

namespace XP_Hotkey.Services;

public class KeyboardHookService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private LowLevelKeyboardProc _proc;
    private IntPtr _hookId = IntPtr.Zero;
    private readonly StringBuilder _buffer = new();
    private readonly SnippetService _snippetService;
    private readonly VariableProcessor _variableProcessor;
    private readonly AppFilterService? _appFilterService;
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly FormDialogService? _formDialogService;
    private AppConfig _config;
    private bool _isProcessing = false;
    private bool _disposed = false;

    public event EventHandler<SnippetExpandedEventArgs>? SnippetExpanded;

    public KeyboardHookService(
        SnippetService snippetService,
        VariableProcessor variableProcessor,
        AppFilterService? appFilterService,
        PerformanceMonitor performanceMonitor,
        AppConfig config,
        FormDialogService? formDialogService = null)
    {
        _snippetService = snippetService;
        _variableProcessor = variableProcessor;
        _appFilterService = appFilterService;
        _performanceMonitor = performanceMonitor;
        _formDialogService = formDialogService;
        _config = config;
        _proc = HookCallback;
    }

    public void Start()
    {
        if (_hookId == IntPtr.Zero)
        {
            _hookId = SetHook(_proc);
        }
    }

    public void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    public void UpdateConfig(AppConfig config)
    {
        _config = config;
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
            GetModuleHandle(curModule?.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && !_isProcessing)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var key = KeyInterop.KeyFromVirtualKey(vkCode);

            if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                ProcessKeyDown(key, vkCode);
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void ProcessKeyDown(Key key, int vkCode)
    {
        // Check if current app is filtered
        if (_appFilterService != null && !_appFilterService.IsAllowed())
        {
            return;
        }

        // Check if trigger key
        bool isTrigger = false;
        if (_config.Triggers.UseSpace && key == Key.Space)
            isTrigger = true;
        else if (_config.Triggers.UseTab && key == Key.Tab)
            isTrigger = true;
        else if (_config.Triggers.UseEnter && key == Key.Enter)
            isTrigger = true;

        if (isTrigger && _buffer.Length > 0)
        {
            var shortcut = _buffer.ToString();
            _buffer.Clear();

            // Find matching snippet
            var snippet = _snippetService.GetSnippetByShortcut(shortcut, false);
            if (snippet != null)
            {
                ExpandSnippet(snippet);
                return; // Consume the trigger key
            }
        }

        // Add character to buffer if it's a printable character
        if (!isTrigger && IsPrintableKey(key))
        {
            var ch = KeyToChar(key);
            if (ch.HasValue)
            {
                _buffer.Append(ch.Value);
                
                // Limit buffer size
                if (_buffer.Length > _config.Triggers.MaxBufferSize)
                {
                    _buffer.Remove(0, _buffer.Length - _config.Triggers.MaxBufferSize);
                }
            }
        }
        else if (key == Key.Back && _buffer.Length > 0)
        {
            _buffer.Length--;
        }
        else if (!IsModifierKey(key) && !isTrigger)
        {
            // Reset buffer on non-printable keys (except modifiers and triggers)
            _buffer.Clear();
        }
    }

    private void ExpandSnippet(Snippet snippet)
    {
        _isProcessing = true;
        _performanceMonitor.StartMeasurement("snippet_expansion", out var stopwatch);

        try
        {
            // Delete the shortcut text
            DeleteText(snippet.Shortcut.Length);

            // Handle form fields
            Dictionary<string, string>? formValues = null;
            if (snippet.FormFields != null && snippet.FormFields.Count > 0 && _formDialogService != null)
            {
                formValues = _formDialogService.ShowFormDialog(snippet.FormFields);
                if (formValues == null)
                {
                    // User cancelled
                    return;
                }
            }

            // Process variables (but keep {cursor} for special handling)
            var expandedText = _variableProcessor.ProcessVariables(snippet.Text, snippet);

            // Process form field values
            if (formValues != null && _formDialogService != null)
            {
                expandedText = _formDialogService.ProcessFormFields(expandedText, formValues);
            }

            // Send the expanded text
            SendText(expandedText);

            // Record usage
            _snippetService.RecordUsage(snippet.Id);

            // Fire event
            SnippetExpanded?.Invoke(this, new SnippetExpandedEventArgs
            {
                Snippet = snippet,
                ExpandedText = expandedText
            });
        }
        finally
        {
            _performanceMonitor.EndMeasurement("snippet_expansion", stopwatch);
            _isProcessing = false;
        }
    }

    private void DeleteText(int length)
    {
        for (int i = 0; i < length; i++)
        {
            SendInput(1, new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    ki = new KEYBDINPUT
                    {
                        wVk = 0x08, // VK_BACK
                        dwFlags = 0
                    }
                }
            }, Marshal.SizeOf(typeof(INPUT)));
            
            Thread.Sleep(10); // Small delay
        }
    }

    private void SendText(string text)
    {
        // Handle {cursor} placeholder - stop sending at cursor position
        var cursorIndex = text.IndexOf("{cursor}", StringComparison.OrdinalIgnoreCase);
        if (cursorIndex >= 0)
        {
            // Send text before cursor
            var textBeforeCursor = text.Substring(0, cursorIndex);
            SendTextInternal(textBeforeCursor);
            // Don't send text after cursor - user can type manually
            return;
        }

        SendTextInternal(text);
    }

    private void SendTextInternal(string text)
    {
        foreach (var ch in text)
        {
            if (ch == '\n')
            {
                // Send Enter for newline
                SendKey(Key.Enter);
            }
            else if (ch == '\r')
            {
                // Ignore carriage return
            }
            else
            {
                SendChar(ch);
            }
            Thread.Sleep(5); // Small delay between characters
        }
    }

    private void SendChar(char ch)
    {
        var vk = VkKeyScan(ch);
        var scanCode = MapVirtualKey((uint)(vk & 0xFF), 0);

        bool shift = (vk & 0x100) != 0;

        if (shift)
        {
            SendKeyDown(Key.LeftShift);
        }

        SendInput(1, new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)(vk & 0xFF),
                    wScan = (ushort)scanCode,
                    dwFlags = 0
                }
            }
        }, Marshal.SizeOf(typeof(INPUT)));

        SendInput(1, new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)(vk & 0xFF),
                    wScan = (ushort)scanCode,
                    dwFlags = KEYEVENTF_KEYUP
                }
            }
        }, Marshal.SizeOf(typeof(INPUT)));

        if (shift)
        {
            SendKeyUp(Key.LeftShift);
        }
    }

    private void SendKey(Key key)
    {
        SendKeyDown(key);
        Thread.Sleep(10);
        SendKeyUp(key);
    }

    private void SendKeyDown(Key key)
    {
        var vk = KeyInterop.VirtualKeyFromKey(key);
        SendInput(1, new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)vk,
                    dwFlags = 0
                }
            }
        }, Marshal.SizeOf(typeof(INPUT)));
    }

    private void SendKeyUp(Key key)
    {
        var vk = KeyInterop.VirtualKeyFromKey(key);
        SendInput(1, new INPUT[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = (ushort)vk,
                    dwFlags = KEYEVENTF_KEYUP
                }
            }
        }, Marshal.SizeOf(typeof(INPUT)));
    }

    private bool IsPrintableKey(Key key)
    {
        return (key >= Key.A && key <= Key.Z) ||
               (key >= Key.D0 && key <= Key.D9) ||
               (key >= Key.NumPad0 && key <= Key.NumPad9) ||
               key == Key.Oem1 || key == Key.Oem2 || key == Key.Oem3 ||
               key == Key.Oem4 || key == Key.Oem5 || key == Key.Oem6 ||
               key == Key.Oem7 || key == Key.Oem8 || key == Key.OemComma ||
               key == Key.OemPeriod || key == Key.OemMinus || key == Key.OemPlus ||
               key == Key.Space;
    }

    private bool IsModifierKey(Key key)
    {
        return key == Key.LeftCtrl || key == Key.RightCtrl ||
               key == Key.LeftAlt || key == Key.RightAlt ||
               key == Key.LeftShift || key == Key.RightShift ||
               key == Key.LWin || key == Key.RWin;
    }

    private char? KeyToChar(Key key)
    {
        if (key >= Key.A && key <= Key.Z)
        {
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var baseChar = (char)('a' + (key - Key.A));
            return shift ? char.ToUpper(baseChar) : baseChar;
        }
        if (key >= Key.D0 && key <= Key.D9)
        {
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var num = key - Key.D0;
            if (shift)
            {
                return ")!@#$%^&*("[num];
            }
            return (char)('0' + num);
        }
        if (key == Key.Space)
            return ' ';
        if (key == Key.OemMinus)
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? '_' : '-';
        if (key == Key.OemPlus)
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? '+' : '=';
        if (key == Key.OemComma)
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? '<' : ',';
        if (key == Key.OemPeriod)
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? '>' : '.';

        return null;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }

    // Windows API declarations
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}

public class SnippetExpandedEventArgs : EventArgs
{
    public Snippet Snippet { get; set; } = null!;
    public string ExpandedText { get; set; } = string.Empty;
}

