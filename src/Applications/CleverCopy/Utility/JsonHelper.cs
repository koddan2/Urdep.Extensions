using System.Text.Json;

namespace CleverCopy.Utility;

internal static class JsonHelper
{
    private static readonly JsonSerializerOptions _JsonSerializerOptions = new() { WriteIndented = true };

    public static string ToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, _JsonSerializerOptions);
    }
}
