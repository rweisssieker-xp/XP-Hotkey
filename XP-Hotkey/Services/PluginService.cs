using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using XP_Hotkey.Plugins;

namespace XP_Hotkey.Services;

public class PluginService
{
    private readonly List<IPlugin> _plugins = new();
    private readonly string _pluginPath;

    public PluginService(string pluginPath = "Data/plugins")
    {
        _pluginPath = pluginPath;
        LoadPlugins();
    }

    public void LoadPlugins()
    {
        if (!Directory.Exists(_pluginPath))
            return;

        foreach (var file in Directory.GetFiles(_pluginPath, "*.dll"))
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var pluginType in pluginTypes)
                {
                    var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
                    _plugins.Add(plugin);
                }
            }
            catch
            {
                // Skip invalid plugins
            }
        }
    }

    public string? ProcessVariable(string variableName, Dictionary<string, string> parameters)
    {
        foreach (var plugin in _plugins)
        {
            var result = plugin.ProcessVariable(variableName, parameters);
            if (result != null)
                return result;
        }
        return null;
    }

    public List<string> GetAllProvidedVariables()
    {
        var variables = new List<string>();
        foreach (var plugin in _plugins)
        {
            variables.AddRange(plugin.ProvidedVariables);
        }
        return variables;
    }

    public List<IPlugin> GetPlugins()
    {
        return _plugins.ToList();
    }
}

