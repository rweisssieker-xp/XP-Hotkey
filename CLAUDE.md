# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

XP-Hotkey is a comprehensive Windows text expander application built with WPF (.NET 9.0). It provides system-wide text expansion triggered by keyboard shortcuts, with advanced features including variables, form dialogs, categories, statistics tracking, clipboard history, plugin system, and system tray integration.

## Building and Running

### Build the application
```bash
dotnet build XP-Hotkey/XP-Hotkey.csproj
```

### Run the application
```bash
dotnet run --project XP-Hotkey/XP-Hotkey.csproj
```

### Build for release
```bash
dotnet build XP-Hotkey/XP-Hotkey.csproj -c Release
```

## Architecture Overview

### Core Components Flow
1. **App.xaml.cs** - Application entry point that initializes all services in dependency order
2. **KeyboardHookService** - Captures system-wide keyboard input using Windows low-level keyboard hooks (SetWindowsHookEx)
3. **SnippetService** - Manages CRUD operations for snippets with JSON persistence
4. **VariableProcessor** - Expands variables in snippet text (date, time, clipboard, random, etc.)
5. **FormDialogService** - Shows parameter input dialogs for snippets with form fields
6. **MainWindow** - Primary UI for managing snippets, backed by MainViewModel

### Service Dependencies
The application initializes services in this specific order (App.xaml.cs:27-52):
1. ConfigService → loads configuration from Data/config.json
2. PerformanceMonitor, ClipboardHistoryService, SnippetService
3. PluginService → loads plugins from Data/plugins
4. VariableProcessor → depends on ClipboardHistoryService and PluginService
5. AppFilterService, FormDialogService, BackupService, ThemeService
6. KeyboardHookService → depends on SnippetService, VariableProcessor, AppFilterService, FormDialogService

### Key Architecture Patterns

**Global Keyboard Hook**: Uses Windows API (SetWindowsHookEx) to intercept all keyboard input system-wide. The hook maintains a character buffer that accumulates typed text and checks for snippet shortcuts when trigger keys (Space, Tab, or Enter) are pressed.

**Variable Processing Pipeline**: Text expansion happens in stages:
1. Snippet matched by shortcut in KeyboardHookService
2. Form dialog shown if snippet has FormFields
3. VariableProcessor processes all {variable} placeholders
4. FormDialogService replaces form field placeholders
5. Text sent via SendInput API, handling {cursor} specially

**Plugin System**: Plugins implement IPlugin interface and can provide custom variables. The PluginService loads plugins from Data/plugins directory. Variables not handled by built-in processors are passed to plugins (VariableProcessor.cs:190-232).

**Thread Safety**: SnippetService uses lock (_lock) around all snippet collection operations since the keyboard hook runs on a different thread than the UI.

## Data Storage

### File Locations
- **Data/snippets.json** - All snippets with metadata (shortcuts, text, categories, statistics)
- **Data/config.json** - Application configuration
- **Data/clipboard_history.json** - Clipboard history entries
- **Data/backups/** - Automatic backups
- **Data/plugins/** - Plugin DLL files

### Snippet Model Structure
Each snippet contains:
- Basic: Id, Shortcut, Text, Description, Enabled
- Organization: Categories (list), Tags (list), IsFavorite
- Behavior: CaseSensitive, UseRegex, Hotkey
- Forms: FormFields (list of FormField objects)
- Tracking: Statistics (UseCount, FirstUsed, LastUsed), Created, Modified, LastUsed

## Variable System

### Built-in Variables
Variables are processed by VariableProcessor in this order:
- `{date}` or `{date:dd.MM.yyyy}` - Current date with optional format
- `{time}` or `{time:HH:mm}` - Current time with optional format
- `{datetime}` or `{datetime:dd.MM.yyyy HH:mm}` - Date and time combined
- `{username}` - Windows username
- `{clipboard}` - Current clipboard content
- `{clipboard_history:N}` - N-th item from clipboard history (1-indexed)
- `{random}` - Random number 0-100
- `{random:min-max}` - Random number in range
- `{uuid}` - New GUID
- `{count}` - Per-snippet counter (tracked by snippet.Id)
- `{count:name}` - Named counter shared across snippets
- `{if:condition:true:false}` - Conditional logic
- `{repeat:text:count}` - Repeat text N times
- `{cursor}` - Special marker; text sending stops here (handled in KeyboardHookService.cs:245-254)

### Plugin Variables
Unrecognized variables are passed to loaded plugins. Plugins can provide custom variables by implementing IPlugin.ProcessVariable().

## Form Dialog System

Snippets can define FormFields to prompt for user input before expansion. When a snippet with FormFields is triggered:
1. KeyboardHookService detects the FormFields (KeyboardHookService.cs:178-187)
2. FormDialogService.ShowFormDialog() displays a WPF dialog
3. User enters values (or cancels)
4. FormDialogService.ProcessFormFields() replaces field placeholders in snippet text
5. Result is sent to the active application

FormField types: text, number (defined in FormField.cs)

## Configuration System

AppConfig contains nested configuration objects:
- **TriggerSettings** - Which keys trigger expansion (Space, Tab, Enter), MaxBufferSize
- **PerformanceSettings** - KeystrokeDelayMs, BackspaceDelayMs for text simulation
- **BackupSettings** - AutoBackupEnabled, BackupIntervalDays, MaxBackups
- **ThemeSettings** - ThemeName, UseDarkMode, AccentColor
- **AppWhitelist/AppBlacklist** - Process names to include/exclude

Configuration is loaded/saved via ConfigService using JsonHelper for async serialization.

## Windows API Integration

### Low-Level Keyboard Hook
The keyboard hook (KeyboardHookService.cs:12-92) uses:
- SetWindowsHookEx(WH_KEYBOARD_LL=13) to install hook
- LowLevelKeyboardProc callback receives all keyboard events
- Hook must call CallNextHookEx to pass events to next handler
- Hook runs on separate thread; use _isProcessing flag to prevent reentrancy

### Text Simulation
Text expansion uses SendInput API (KeyboardHookService.cs:223-330):
- DeleteText() sends VK_BACK (0x08) to erase shortcut
- SendChar() uses VkKeyScan to convert char to virtual key code
- Handles Shift modifier for uppercase/symbols
- SendKey() for special keys like Enter (newlines)

## System Tray Integration

The application can minimize to system tray (App.xaml.cs:99-175):
- NotifyIcon created with custom blue "XP" icon
- Context menu provides: Open Window, Settings, Exit
- DoubleClick restores window
- Window closing is intercepted if MinimizeToTray is enabled

## Important Development Notes

### When Adding New Variables
1. Add processing method in VariableProcessor.cs
2. Add variable name to builtInVariables HashSet (line 203) to prevent plugin processing
3. Call the processor in ProcessVariables() method (lines 33-42)

### When Modifying Snippet Model
1. Update Snippet.cs model class
2. Update SnippetService load/save logic if needed
3. Update UI in MainViewModel and SnippetEditViewModel
4. Consider migration logic for existing snippets.json files

### When Adding New Services
1. Initialize in App.OnStartup() in correct dependency order
2. Pass to MainWindow constructor if UI needs access
3. Consider thread safety if accessed from keyboard hook thread
4. Implement IDisposable if service manages unmanaged resources

### Thread Safety Considerations
- Keyboard hook callback runs on Windows hook thread
- UI operations must be dispatched to UI thread
- SnippetService methods are thread-safe (use lock)
- Be careful when accessing WPF controls from hook callback

### Performance Considerations
- Keyboard hook must be fast to avoid input lag
- Use _isProcessing flag to prevent reentrant processing
- PerformanceMonitor tracks snippet expansion time
- KeystrokeDelayMs and BackspaceDelayMs configurable for compatibility

## Common Tasks

### Testing Text Expansion
1. Run the application
2. Type a snippet shortcut (e.g., "test")
3. Press Space or Tab (depending on trigger settings)
4. The shortcut should be deleted and replaced with expanded text

### Debugging Keyboard Hook Issues
- Check System.Diagnostics.Debug.WriteLine output
- Verify hook installed successfully (SetHook doesn't return IntPtr.Zero)
- Check _isProcessing flag isn't stuck true
- Test with simple snippets first (no variables/forms)

### Adding a New Built-in Variable
Example for `{year}`:
```csharp
// In VariableProcessor.cs
private string ProcessYearVariables(string text)
{
    return text.Replace("{year}", DateTime.Now.Year.ToString());
}

// Add to ProcessVariables():
result = ProcessYearVariables(result);

// Add to builtInVariables HashSet:
"year"
```

## Project Structure Notes

- **Models/** - Data classes (Snippet, AppConfig, FormField, etc.)
- **Services/** - Business logic (KeyboardHookService, SnippetService, VariableProcessor, etc.)
- **ViewModels/** - MVVM view models for UI
- **Views/** - WPF windows and dialogs
- **Controls/** - Reusable WPF user controls
- **Utilities/** - Helpers (JsonHelper, EncryptionHelper, PerformanceMonitor)
- **Plugins/** - Plugin interface (IPlugin.cs)
- **Data/** - Runtime data directory (created at runtime)
