using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;

namespace CleverCopy;

internal static class YamlHelper
{
    public static ISerializer GetSerializer() => new SerializerBuilder()
            .WithTypeConverter(new DateTimeOffsetConverter())
            .Build();

    public static IDeserializer GetDeserializer() => new DeserializerBuilder()
            .WithTypeConverter(new DateTimeOffsetConverter())
            .Build();

    public static void SerializeToWriter<T>(TextWriter writer, T obj)
    {
        var serializer = GetSerializer();
        serializer.Serialize(writer, obj);
    }

    public static void SerializeToFile<T>(string path, T obj)
    {
        using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
        using StreamWriter streamWriter = new(stream);
        SerializeToWriter(streamWriter, obj);
    }

    public static T? DeserializeFromReader<T>(TextReader reader)
    {
        var deserializer = GetDeserializer();
        return deserializer.Deserialize<T>(reader);
    }

    public static T? DeserializeFromFile<T>(string path)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        using StreamReader streamReader = new(stream);
        return DeserializeFromReader<T>(streamReader);
    }
}
