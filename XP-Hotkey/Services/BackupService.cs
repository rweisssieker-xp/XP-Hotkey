using System.IO;
using System.IO.Compression;
using XP_Hotkey.Models;
using XP_Hotkey.Utilities;

namespace XP_Hotkey.Services;

public class BackupService
{
    private readonly string _backupPath;
    private readonly SnippetService _snippetService;
    private readonly ConfigService _configService;

    public BackupService(string dataPath, SnippetService snippetService, ConfigService configService)
    {
        _backupPath = Path.Combine(dataPath, "backups");
        _snippetService = snippetService;
        _configService = configService;
        EnsureBackupDirectory();
    }

    private void EnsureBackupDirectory()
    {
        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }
    }

    public string CreateBackup(string? description = null)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"backup_{timestamp}.zip";
        var filePath = Path.Combine(_backupPath, fileName);

        using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Create))
        {
            // Backup snippets
            var snippetsJson = JsonHelper.Serialize(_snippetService.GetAllSnippets());
            var snippetsEntry = archive.CreateEntry("snippets.json");
            using (var writer = new StreamWriter(snippetsEntry.Open()))
            {
                writer.Write(snippetsJson);
            }

            // Backup config
            var configJson = JsonHelper.Serialize(_configService.GetConfig());
            var configEntry = archive.CreateEntry("config.json");
            using (var writer = new StreamWriter(configEntry.Open()))
            {
                writer.Write(configJson);
            }

            // Backup info
            var backupInfo = new BackupInfo
            {
                FileName = fileName,
                Created = DateTime.Now,
                Size = new FileInfo(filePath).Length,
                Description = description,
                SnippetCount = _snippetService.GetAllSnippets().Count
            };
            var infoJson = JsonHelper.Serialize(backupInfo);
            var infoEntry = archive.CreateEntry("backup_info.json");
            using (var writer = new StreamWriter(infoEntry.Open()))
            {
                writer.Write(infoJson);
            }
        }

        CleanupOldBackups();
        return filePath;
    }

    public void RestoreBackup(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("Backup file not found");

        using (var archive = ZipFile.OpenRead(backupFilePath))
        {
            // Restore snippets
            var snippetsEntry = archive.GetEntry("snippets.json");
            if (snippetsEntry != null)
            {
                using (var reader = new StreamReader(snippetsEntry.Open()))
                {
                    var json = reader.ReadToEnd();
                    var snippets = JsonHelper.Deserialize<List<Snippet>>(json);
                    if (snippets != null)
                    {
                        // Import snippets (will merge with existing)
                        _snippetService.ImportFromJson(Path.Combine(_backupPath, "temp_snippets.json"));
                        File.WriteAllText(Path.Combine(_backupPath, "temp_snippets.json"), json);
                        _snippetService.ImportFromJson(Path.Combine(_backupPath, "temp_snippets.json"));
                        File.Delete(Path.Combine(_backupPath, "temp_snippets.json"));
                    }
                }
            }

            // Restore config
            var configEntry = archive.GetEntry("config.json");
            if (configEntry != null)
            {
                using (var reader = new StreamReader(configEntry.Open()))
                {
                    var json = reader.ReadToEnd();
                    var config = JsonHelper.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        _configService.SaveConfig(config);
                    }
                }
            }
        }
    }

    public List<BackupInfo> GetBackups()
    {
        var backups = new List<BackupInfo>();
        if (!Directory.Exists(_backupPath))
            return backups;

        foreach (var file in Directory.GetFiles(_backupPath, "backup_*.zip"))
        {
            try
            {
                using (var archive = ZipFile.OpenRead(file))
                {
                    var infoEntry = archive.GetEntry("backup_info.json");
                    if (infoEntry != null)
                    {
                        using (var reader = new StreamReader(infoEntry.Open()))
                        {
                            var json = reader.ReadToEnd();
                            var info = JsonHelper.Deserialize<BackupInfo>(json);
                            if (info != null)
                            {
                                backups.Add(info);
                            }
                        }
                    }
                    else
                    {
                        // Create info from file
                        backups.Add(new BackupInfo
                        {
                            FileName = Path.GetFileName(file),
                            Created = File.GetCreationTime(file),
                            Size = new FileInfo(file).Length
                        });
                    }
                }
            }
            catch
            {
                // Skip invalid backups
            }
        }

        return backups.OrderByDescending(b => b.Created).ToList();
    }

    private void CleanupOldBackups()
    {
        var config = _configService.GetConfig();
        var maxBackups = config.Backup.MaxBackups;

        var backups = GetBackups();
        if (backups.Count > maxBackups)
        {
            var toDelete = backups.Skip(maxBackups).ToList();
            foreach (var backup in toDelete)
            {
                var filePath = Path.Combine(_backupPath, backup.FileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }

    public void ExportToJson(string filePath)
    {
        _snippetService.ExportToJson(filePath);
    }

    public void ExportToCsv(string filePath)
    {
        var snippets = _snippetService.GetAllSnippets();
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Shortcut,Text,Description,Categories,Tags,IsFavorite");
            foreach (var snippet in snippets)
            {
                var categories = string.Join(";", snippet.Categories);
                var tags = string.Join(";", snippet.Tags);
                writer.WriteLine($"\"{snippet.Shortcut}\",\"{snippet.Text.Replace("\"", "\"\"")}\",\"{snippet.Description?.Replace("\"", "\"\"") ?? ""}\",\"{categories}\",\"{tags}\",{snippet.IsFavorite}");
            }
        }
    }
}

