namespace Urdep.Extensions.Data;

using System.Collections.Generic;

/// <summary>
/// Generic extension methods for IEnumerable{T} instances.
/// </summary>
public static class EnumerableExtension
{
    /**
    See https://stackoverflow.com/a/39997157
    Example:
    <example>
    <code>
    foreach (var (item, index) in collection.WithIndex())
    {
        Debug.WriteLine($"{index}: {item}");
    }
    </code>
    </example>
    **/
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
        self.Select((item, index) => (item, index));
}
