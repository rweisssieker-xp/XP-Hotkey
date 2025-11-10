<!-- b07b0aa2-050b-44d8-a91b-d168574ad60b 49e23d37-da94-4d5a-b80b-d3a6931bec75 -->
# WPF Text Expander Anwendung - Vollversion

## Übersicht

Eine umfassende, systemweite Text-Expander-Anwendung für Windows mit professionellen Features: erweiterte Variablen, Formular-Dialoge, Kategorien, Statistiken, Systemtray-Integration, Hotkeys, Clipboard-History, Plugin-System, Themes, Verschlüsselung und mehr.

## Technische Architektur

### Hauptkomponenten

1. **Global Keyboard Hook** - Systemweite Tastatur-Erkennung mit Windows API
2. **Snippet Manager** - Erweiterte Verwaltung mit Kategorien, Tags, Statistiken
3. **Variable Processor** - Umfangreiche Variablen-Unterstützung ({date}, {time}, {random}, {uuid}, {cursor}, {count}, etc.)
4. **Formular-Engine** - Dialog-System für parametrierte Snippets
5. **WPF GUI** - Vollständiges Verwaltungsinterface mit Suche, Filter, Kategorien
6. **Systemtray Integration** - Minimieren auf Tray mit Quick-Access
7. **Hotkey Manager** - Globale Shortcuts für häufige Snippets
8. **Clipboard History** - Erweiterte Zwischenablage-Verwaltung
9. **Plugin System** - Erweiterbare Architektur für Custom-Variablen
10. **Backup/Restore** - Automatische Backups und Import/Export
11. **App Filter** - Whitelist/Blacklist für bestimmte Anwendungen
12. **Theme Engine** - Dark Mode und Theme-Unterstützung

## Erweiterte Dateistruktur

```
XP-Hotkey/
├── XP-Hotkey.csproj
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── Views/
│   ├── SnippetEditDialog.xaml      # Dialog zum Bearbeiten von Snippets
│   ├── FormDialog.xaml              # Formular-Dialog für Parameter
│   ├── SettingsWindow.xaml          # Einstellungsfenster
│   ├── QuickAddDialog.xaml          # Schnell-Dialog für neue Snippets
│   └── StatisticsWindow.xaml       # Statistik-Anzeige
├── Models/
│   ├── Snippet.cs                   # Erweitertes Snippet-Modell
│   ├── AppConfig.cs                 # Vollständige Konfiguration
│   ├── Category.cs                  # Kategorie-Modell
│   ├── FormField.cs                 # Formular-Feld-Definition
│   ├── BackupInfo.cs                # Backup-Metadaten
│   └── SnippetStatistics.cs         # Statistik-Daten
├── Services/
│   ├── KeyboardHookService.cs       # Global Keyboard Hook
│   ├── SnippetService.cs            # Erweiterte Snippet-Verwaltung
│   ├── VariableProcessor.cs         # Umfangreicher Variablen-Processor
│   ├── FormDialogService.cs         # Formular-Dialog-Engine
│   ├── ClipboardHistoryService.cs   # Clipboard-History-Verwaltung
│   ├── HotkeyService.cs             # Hotkey-Management
│   ├── PluginService.cs             # Plugin-System
│   ├── BackupService.cs             # Backup/Restore/Import/Export
│   ├── AppFilterService.cs          # App-Whitelist/Blacklist
│   └── ThemeService.cs              # Theme-Management
├── ViewModels/
│   ├── MainViewModel.cs             # Haupt-ViewModel
│   ├── SnippetEditViewModel.cs      # ViewModel für Snippet-Bearbeitung
│   └── SettingsViewModel.cs         # Einstellungs-ViewModel
├── Controls/
│   ├── SnippetListControl.xaml      # Wiederverwendbare Snippet-Liste
│   └── CategoryTreeControl.xaml     # Kategorie-Baumansicht
├── Plugins/
│   └── IPlugin.cs                   # Plugin-Interface
├── Utilities/
│   ├── JsonHelper.cs                # JSON-Hilfsfunktionen
│   ├── EncryptionHelper.cs          # Verschlüsselung für sensible Daten
│   └── PerformanceMonitor.cs        # Performance-Tracking
├── Data/
│   ├── snippets.json                # Snippets (verschlüsselt optional)
│   ├── config.json                  # Konfiguration
│   ├── clipboard_history.json       # Clipboard-History
│   ├── backups/                     # Backup-Verzeichnis
│   └── plugins/                     # Plugin-Verzeichnis
└── Resources/
    ├── Icons/                       # App-Icons
    └── Themes/                      # Theme-Definitionen
```

## Feature-Implementierung

### Phase 1: Kern-Funktionalität

- Projekt-Setup mit allen Dependencies
- Global Keyboard Hook mit Trigger-Erkennung
- Basis-Snippet-Verwaltung (CRUD)
- Erweiterte Variablen: {date}, {time}, {username}, {clipboard}, {random}, {uuid}, {cursor}, {count}
- Basis-GUI für Snippet-Verwaltung

### Phase 2: Erweiterte Verwaltung

- Kategorien/Tags-System
- Suche und Filter-Funktionen
- Statistiken-Tracking (Nutzungshäufigkeit)
- Favoriten-System
- Duplizieren-Funktion
- Import/Export (JSON/CSV)

### Phase 3: Formular-Dialoge

- Formular-Engine mit Feld-Definitionen
- Dialog-Generator für parametrierte Snippets
- Validierung und Fehlerbehandlung
- Beispiel: Kundennummer-Eingabe

### Phase 4: UI/UX Verbesserungen

- Systemtray-Integration mit NotifyIcon
- Quick-Access-Menü im Tray
- Hotkey-System für häufige Snippets
- Live-Vorschau beim Tippen
- Undo/Redo-Funktionalität
- Quick-Add-Dialog (Hotkey-basiert)
- Dark Mode / Theme-System

### Phase 5: Erweiterte Features

- Clipboard-History-Service
- App-Whitelist/Blacklist (Prozess-Erkennung)
- Performance-Monitoring
- Auto-Backup-System
- Verschlüsselung für sensible Snippets
- Regex-Matching-Option
- Case-Sensitivity-Optionen

### Phase 6: Plugin-System

- Plugin-Interface (IPlugin)
- Plugin-Loader und -Manager
- Beispiel-Plugins für Custom-Variablen
- Plugin-Konfiguration

## Erweiterte Variablen

### Standard-Variablen

- `{date}` - Aktuelles Datum (formatierbar: {date:dd.MM.yyyy})
- `{time}` - Aktuelle Uhrzeit (formatierbar: {time:HH:mm})
- `{datetime}` - Datum und Zeit kombiniert
- `{username}` - Windows-Benutzername
- `{clipboard}` - Aktueller Clipboard-Inhalt
- `{clipboard_history:N}` - N-ter Eintrag aus Clipboard-History
- `{random}` - Zufallszahl (0-100)
- `{random:min-max}` - Zufallszahl im Bereich
- `{uuid}` - Eindeutige UUID/GUID
- `{cursor}` - Platzhalter für manuelle Eingabe (Cursor bleibt stehen)
- `{count}` - Inkrementierender Zähler pro Snippet
- `{count:name}` - Benannter Zähler

### Erweiterte Syntax

- `{date:format}` - Formatierbares Datum
- `{if:condition:true:false}` - Bedingte Logik
- `{repeat:text:count}` - Text-Wiederholung

## Snippet-Modell (erweitert)

```csharp
public class Snippet {
    public string Id { get; set; }
    public string Shortcut { get; set; }
    public string Text { get; set; }
    public string Description { get; set; }
    public List<string> Categories { get; set; }
    public List<string> Tags { get; set; }
    public bool IsFavorite { get; set; }
    public bool CaseSensitive { get; set; }
    public bool UseRegex { get; set; }
    public List<FormField> FormFields { get; set; }
    public SnippetStatistics Statistics { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public DateTime LastUsed { get; set; }
}
```

## Formular-Dialoge

Beispiel-Snippet mit Formular:

```json
{
  "shortcut": "kunde",
  "text": "Sehr geehrte/r {name},\nKundennummer: {kundennr}",
  "formFields": [
    {
      "name": "name",
      "label": "Name",
      "type": "text",
      "required": true
    },
    {
      "name": "kundennr",
      "label": "Kundennummer",
      "type": "number",
      "required": true
    }
  ]
}
```

## Konfiguration (erweitert)

```csharp
public class AppConfig {
    public TriggerSettings Triggers { get; set; }
    public bool StartMinimized { get; set; }
    public bool MinimizeToTray { get; set; }
    public ThemeSettings Theme { get; set; }
    public List<string> AppWhitelist { get; set; }
    public List<string> AppBlacklist { get; set; }
    public BackupSettings Backup { get; set; }
    public bool EncryptSensitiveSnippets { get; set; }
    public PerformanceSettings Performance { get; set; }
}
```

## Technische Details

### Keyboard Hook Optimierung

- Effiziente Buffer-Verwaltung
- Performance-Monitoring für Latenz
- Thread-sichere Implementierung

### Plugin-System

```csharp
public interface IPlugin {
    string Name { get; }
    string Version { get; }
    void Initialize(IServiceProvider services);
    string ProcessVariable(string variableName, Dictionary<string, string> parameters);
    List<string> ProvidedVariables { get; }
}
```

### Backup-System

- Automatische tägliche Backups
- Manuelle Backup-Erstellung
- Export in JSON/CSV
- Restore-Funktionalität

### Verschlüsselung

- AES-Verschlüsselung für sensible Snippets
- Master-Passwort-Schutz
- Optionale Verschlüsselung pro Snippet

### To-dos

- [ ] WPF-Projekt erstellen mit .NET SDK, Projektdatei konfigurieren, NuGet-Pakete hinzufügen
- [ ] Snippet.cs und AppConfig.cs Modelle erstellen für Datenstruktur
- [ ] KeyboardHookService.cs implementieren mit globalem Windows Hook (SetWindowsHookEx)
- [ ] SnippetService.cs implementieren für JSON-basierte CRUD-Operationen
- [ ] VariableProcessor.cs implementieren für {date}, {username}, {clipboard} Ersetzung
- [ ] MainWindow.xaml und Code-Behind erstellen mit Snippet-Liste, Add/Edit/Delete Funktionen
- [ ] Konfigurations-Manager für Trigger-Einstellungen (Leertaste/Tab) implementieren
- [ ] Alle Komponenten integrieren: Hook mit Snippet-Service verbinden, GUI mit Services verknüpfen