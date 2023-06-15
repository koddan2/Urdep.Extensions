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

    /// <summary>
    /// Gets an instance of <see cref="Euid"/> from an array of bytes.
    /// </summary>
    /// <param name="bytes">The byte array.</param>
    /// <returns>The instance.</returns>
    public static Euid FromBytes(byte[] bytes) => new(Base62.EncodingExtensions.ToBase62(bytes));

    /// <summary>
    /// Gets the byte array that represents this value.
    /// </summary>
    /// <returns>The byte array</returns>
    public byte[] GetByteArray() => Base62.EncodingExtensions.FromBase62(Value);

    /// <summary>
    /// Implicitly converts an <see cref="Euid"/> to a string.
    /// </summary>
    /// <param name="euid">The Euid value.</param>
    public static implicit operator string(Euid euid) => euid.Value;

    /// <summary>
    /// Implicitly converts a string to an <see cref="Euid"/>.
    /// </summary>
    /// <param name="str">The string value.</param>
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
    /// <param name="euid">The Euid value.</param>
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

    /// <summary>
    /// ToString method.
    /// </summary>
    /// <returns>The string represenation.</returns>
    public override string ToString() => this;
}

/// <summary>
/// A JSON (System.Text.Json) converter for <see cref="Euid"/>.
/// </summary>
public class EuidJsonConverter : JsonConverter<Euid>
{
    /// <summary>
    /// Read the value for the specified reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public override Euid Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => reader.GetString()!;

    /// <summary>
    /// Write to the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The options.</param>
    public override void Write(Utf8JsonWriter writer, Euid value, JsonSerializerOptions options) =>
        writer.WriteStringValue((string)value);
}
