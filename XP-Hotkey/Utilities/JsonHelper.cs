using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XP_Hotkey.Utilities;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static async Task<T?> DeserializeFromFileAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        return Deserialize<T>(json);
    }

    public static async Task SerializeToFileAsync<T>(T obj, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = Serialize(obj);
        await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }
}
