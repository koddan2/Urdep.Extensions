using System.Diagnostics;
using System.Reflection;
using Urdep.Extensions.Augmentation;

namespace Urdep.Extensions.Data;

/// <summary>
/// Object extensions
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Transform into an object, runtime reflection
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="source">The source data</param>
    /// <returns>The result</returns>
    public static T TransformInto<T>(this IDictionary<string, object?> source)
        where T : class, new()
    {
        var result = new T();
        var type = typeof(T);

        foreach (var item in source)
        {
            var property = type.GetProperty(item.Key);
            if (property == null)
            {
#if DEBUG
                Debug.WriteLine(
                    $"Tried to read PropertyInfo {item.Key} on Type {type.FullName} which didn't work."
                );
#endif
                continue;
            }
            property.SetValue(result, item.Value, null);
        }

        return result;
    }

    private const BindingFlags _DefaultBindingFlags =
        BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// Construct a dictionary from the given instance. Runtime reflection.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The source of the data.</param>
    /// <param name="bindingAttr">The binding attributes.</param>
    /// <returns>The dictionary</returns>
    public static IDictionary<string, object?> AsDictionary<T>(
        this IAugmented<T> source,
        BindingFlags bindingAttr = _DefaultBindingFlags
    )
    {
        return typeof(T)
            .GetProperties(bindingAttr)
            .ToDictionary(
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source.Value, null)
            );
    }
}

/// <summary>
/// Meta extensions
/// </summary>
public static class MetaExtensions
{
    /// <summary>
    /// In-place update of object - only used for causing a side effect.
    /// </summary>
    /// <example>
    /// <code>
    /// var aug = Augment.Ref(new [] {0, 1, 2})
    ///   .Tap(arr => arr[0] = 99);
    /// // aug ~= (Augmented) new [] { 99, 1, 2 };
    /// </code>
    /// </example>
    /// <typeparam name="T">The relevant type.</typeparam>
    /// <param name="augmented">The augmented object.</param>
    /// <param name="mutator">The delegate that does the side-effect(s).</param>
    /// <returns>The mutated value</returns>
    public static IAugmented<T> Tap<T>(this IAugmented<T> augmented, Action<T> mutator)
    {
        mutator(augmented.Value);
        return augmented;
    }
}
