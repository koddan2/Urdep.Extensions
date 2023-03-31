using System.Text.Json;
using System.Text.Json.Nodes;

namespace Urdep.Extensions.Data;

public static class DynamicJsonHelperExtensions
{
    public static void Map<T>(this DynamicJsonHelper instance, string name) =>
        instance.Map(name, typeof(T));

    public static DynamicJsonHelper MapMany(
        this DynamicJsonHelper instance,
        params (string, Type)[] values
    )
    {
        foreach (var value in values)
        {
            instance.Map(value.Item1, value.Item2);
        }

        return instance;
    }

    public static DynamicJsonHelper MapMany(
        this DynamicJsonHelper instance,
        IDictionary<string, Type> dict
    )
    {
        foreach (var kvp in dict)
        {
            instance.Map(kvp.Key, kvp.Value);
        }

        return instance;
    }
}

/// <summary>
/// <code>
/// var helper = new DynamicJsonHelper("TypeName");
/// helper.Map("Thing", typeof(Thing));
/// var json = "{\"TypeName\": \"Thing\", ...}";
/// var jsonNode = JsonSerializer.Deserialize&lt;JsonNode&gt;(json);
/// if (helper.Is&lt;Thing&gt;(jsonNode)) {
///   Thing? typedObj = jsonNode.Deserialize&lt;Thing?&gt;();
///   // use typedObj
/// }
/// </code>
/// </summary>
public class DynamicJsonHelper
{
    protected readonly Dictionary<string, Type> _typeMap = new();

    public DynamicJsonHelper(string keyProperty)
    {
        if (string.IsNullOrWhiteSpace(keyProperty))
        {
            throw new ArgumentException("Parameter may not be null or empty", nameof(keyProperty));
        }
        KeyProperty = keyProperty;
    }

    public string KeyProperty { get; }

    public void Map(string name, Type type) => _typeMap[name] = type;

    public Type Which(string name) => _typeMap[name];

    /// <summary>
    /// Test whether the JsonNode indicates that it is of the given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="node"></param>
    /// <returns>True if the JsonNode indicates that it is of the given type, otherwise false.</returns>
    /// <exception cref="InvalidDataException">
    /// If the JsonNode does not contain a property by the name equivalent to the value of <see cref="KeyProperty"/>
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// If the <see cref="KeyProperty"/> value is not mapped to a type.
    /// </exception>
    public bool Is<T>(JsonNode node)
    {
        var typeName =
            (node[KeyProperty]?.GetValue<string>())
            ?? throw new InvalidDataException(
                $"Unable to extract key ({KeyProperty}) from JsonNode"
            );

        if (_typeMap.TryGetValue(typeName, out var type))
        {
            return type == typeof(T);
        }

        throw new KeyNotFoundException($"Key {typeName} was not found in the type map.");
    }
}
