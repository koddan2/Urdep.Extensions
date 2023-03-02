using System.Text.Json;
using System.Text.Json.Serialization;

namespace Urdep.Extensions.Data;

/// <summary>
/// A struct simplifying using bytes as a somewhat human-readable identifier.
/// </summary>
/// <param name="Value">The string value.</param>
[JsonConverter(typeof(EuidJsonConverter))]
public readonly record struct Euid(string Value)
{
    /// <summary>
    /// Makes an <see cref="Euid"/> from a string.
    /// </summary>
    /// <param name="str">The string value.</param>
    /// <returns>The Euid value.</returns>
    public static Euid FromString(string str) => new(str);

    public static Euid FromBytes(byte[] bytes) => new(Base62.EncodingExtensions.ToBase62(bytes));

    public byte[] GetByteArray() => Base62.EncodingExtensions.FromBase62(Value);

    /// <summary>
    /// Implicitly converts an <see cref="Euid"/> to a string.
    /// </summary>
    /// <param name="euid">The Euid value.</param>
    public static implicit operator string(Euid euid) => euid.Value;

    /// <summary>
    /// Implicitly converts a string to an <see cref="Euid"/>.
    /// </summary>
    /// <param name="euid">The string value.</param>
    public static implicit operator Euid(string str) => new(str);

    /// <summary>
    /// Implicitly converts a <see cref="System.Guid"/> to an <see cref="Euid"/>.
    /// </summary>
    /// <param name="g">The Guid value.</param>
    public static implicit operator Euid(Guid g) =>
        new(Base62.EncodingExtensions.ToBase62(g.ToByteArray()));

    /// <summary>
    /// Implicitly converts an <see cref="Euid"/> to a <see cref="System.Guid"/>.
    /// </summary>
    /// <param name="g">The Euid value.</param>
    public static implicit operator Guid(Euid euid) =>
        new(Base62.EncodingExtensions.FromBase62(euid.Value));

    /// <summary>
    /// Makes a new random <see cref="Euid"/> using <see cref="System.Guid.NewGuid"/>.
    /// </summary>
    /// <returns>The newly created, random, Euid.</returns>
    public static Euid New() => Guid.NewGuid();

    /// <summary>
    /// The empty <see cref="Euid"/> value (equivalent to <see cref="System.Guid.Empty"/>).
    /// </summary>
    public readonly static Euid Empty = Guid.Empty;

    public override string ToString() => this;
}

public class EuidJsonConverter : JsonConverter<Euid>
{
    public override Euid Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => reader.GetString()!;

    public override void Write(Utf8JsonWriter writer, Euid value, JsonSerializerOptions options) =>
        writer.WriteStringValue((string)value);
}
