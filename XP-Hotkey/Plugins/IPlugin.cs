using System.Collections.Generic;

namespace XP_Hotkey.Plugins;

public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    void Initialize(IServiceProvider services);
    string? ProcessVariable(string variableName, Dictionary<string, string> parameters);
    List<string> ProvidedVariables { get; }
}

