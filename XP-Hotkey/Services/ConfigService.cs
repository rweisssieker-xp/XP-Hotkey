using System.IO;
using XP_Hotkey.Models;
using XP_Hotkey.Utilities;

namespace XP_Hotkey.Services;

public class ConfigService
{
    private readonly string _configPath;
    private AppConfig _config = null!;

    public ConfigService(string dataPath = "Data")
    {
        _configPath = Path.Combine(dataPath, "config.json");
        LoadConfig();
    }

    public AppConfig GetConfig()
    {
        return _config;
    }

    public void SaveConfig(AppConfig config)
    {
        _config = config;
        SaveConfig();
    }

    public void UpdateConfig(Action<AppConfig> updateAction)
    {
        updateAction(_config);
        SaveConfig();
    }

    private void LoadConfig()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var config = JsonHelper.DeserializeFromFileAsync<AppConfig>(_configPath).Result;
                if (config != null)
                {
                    _config = config;
                }
                else
                {
                    _config = new AppConfig();
                }
            }
            catch
            {
                _config = new AppConfig();
            }
        }
        else
        {
            _config = new AppConfig();
            SaveConfig();
        }
    }

    private void SaveConfig()
    {
        try
        {
            JsonHelper.SerializeToFileAsync(_config, _configPath).Wait();
        }
        catch
        {
            // Log error in production
        }
    }
}

