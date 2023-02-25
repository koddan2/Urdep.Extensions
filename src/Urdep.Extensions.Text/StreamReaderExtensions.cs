namespace Urdep.Extensions.Text;

public enum TrimMode
{
    None,
    Trim,
    TrimStart,
    TrimEnd,
}

public record StreamReaderSkipBlocksState(
    string BeginSkipBlock,
    string EndSkipBlock,
    TrimMode TrimMode = TrimMode.None
)
{
    // https://stackoverflow.com/a/29180781
    public bool Skipping { get; set; } = false;
};

public static class StreamReaderExtensions
{
    public static string? ReadLine(this (StreamReader, StreamReaderSkipBlocksState) product)
    {
        (StreamReader reader, StreamReaderSkipBlocksState state) = product;
        var line = state.TrimMode switch
        {
            TrimMode.None => reader.ReadLine(),
            TrimMode.TrimStart => reader.ReadLine()?.TrimStart(),
            TrimMode.TrimEnd => reader.ReadLine()?.TrimEnd(),
            TrimMode.Trim => reader.ReadLine()?.Trim(),
        };

        if (line == state.BeginSkipBlock)
        {
            state.Skipping = true;
            return default;
        }
        else if (line == state.EndSkipBlock)
        {
            state.Skipping = false;
            return default;
        }
        else if (state.Skipping)
        {
            return default;
        }
        else
        {
            return line;
        }
    }
}
