using System.Diagnostics;
using System.Reflection;
using Urdep.Extensions.Augmentation;

namespace Urdep.Extensions.Data;

public static class ObjectExtensions
{
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
