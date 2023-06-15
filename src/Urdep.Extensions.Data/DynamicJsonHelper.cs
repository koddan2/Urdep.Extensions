using System.Text.Json;
using System.Text.Json.Nodes;

namespace Urdep.Extensions.Data;

/// <summary>
/// Extension methods for <see cref="DynamicJsonHelper"/>.
/// </summary>
public static class DynamicJsonHelperExtensions
{
    /// <summary>
    /// Map a type.
    /// </summary>
    /// <typeparam name="T">The type to map.</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="name">The name of the type.</param>
    public static void Map<T>(this DynamicJsonHelper instance, string name) =>
        instance.Map(name, typeof(T));

    /// <summary>
    /// Map many at once.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="values">The values.</param>
    /// <returns>The instance.</returns>
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

    /// <summary>
    /// Maps many mappings at once.
    /// </summary>
    /// <param name="instance">The instance to add mappings to.</param>
    /// <param name="dict">The dictionary containing the mappings.</param>
    /// <returns>The same instance.</returns>
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
    /// <summary>
    /// The type map instance.
    /// </summary>
    protected readonly Dictionary<string, Type> _typeMap = new();

    /// <summary>
    /// Constructs an instance.
    /// </summary>
    /// <param name="keyProperty">The name of the key property.</param>
    /// <exception cref="ArgumentException">If the key property is null or empty.</exception>
    public DynamicJsonHelper(string keyProperty)
    {
        if (string.IsNullOrWhiteSpace(keyProperty))
        {
            throw new ArgumentException("Parameter may not be null or empty", nameof(keyProperty));
        }
        KeyProperty = keyProperty;
    }

    /// <summary>
    /// The key property name.
    /// </summary>
    public string KeyProperty { get; }

    /// <summary>
    /// Maps the name to the type.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="type">The type.</param>
    public void Map(string name, Type type) => _typeMap[name] = type;

    /// <summary>
    /// Gets the type with the given name. Will throw if not found.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The type, if found (otherwise an exception is thrown.)</returns>
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
