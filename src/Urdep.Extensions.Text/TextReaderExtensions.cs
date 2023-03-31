using System.Text;

namespace Urdep.Extensions.Text;

public enum TrimMode
{
    None,
    Trim,
    TrimStart,
    TrimEnd,
}

public record TextReaderBlockSkipState(
    string BeginSkipBlock,
    string EndSkipBlock,
    TrimMode TrimMode = TrimMode.None
);

public static class TextReaderExtensions
{
    public static string? ReadLine(this TextReaderBlockSkipState state, TextReader reader)
    {
        var result = state.TrimMode switch
        {
            TrimMode.None => reader.ReadLine(),
            TrimMode.TrimStart => reader.ReadLine()?.TrimStart(),
            TrimMode.TrimEnd => reader.ReadLine()?.TrimEnd(),
            TrimMode.Trim => reader.ReadLine()?.Trim(),
            _ => null,
        };

        return result;
    }

    public static void ReadInto(
        this TextReaderBlockSkipState state,
        StringBuilder sb,
        TextReader reader
    )
    {
        bool skipping = false;

        var line = state.ReadLine(reader);
        while (line is not null)
        {
            if (line == state.BeginSkipBlock)
            {
                skipping = true;
                line = state.ReadLine(reader);
                continue;
            }
            else if (line == state.EndSkipBlock)
            {
                skipping = false;
                line = state.ReadLine(reader);
                continue;
            }
            if (skipping)
            {
                line = state.ReadLine(reader);
                continue;
            }

            sb.AppendLine(line);
            line = state.ReadLine(reader);
        }
    }
}
