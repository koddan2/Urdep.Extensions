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
