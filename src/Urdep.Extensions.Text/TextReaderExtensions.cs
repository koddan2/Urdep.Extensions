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
)
{
    public bool Skipping { get; set; } = false;
};

public readonly record struct TextReaderBlockSkipResult(bool Skip, string? Line);

public static class TextReaderExtensions
{
    public static string ReadAll(this TextReaderBlockSkipState state, TextReader reader)
    {
        var sb = new System.Text.StringBuilder();
        TextReaderBlockSkipResult readResult;
        do
        {
            readResult = state.ReadLine(reader);
            if (!readResult.Skip)
            {
                sb.AppendLine(readResult.Line);
            }
        } while (readResult.Line is not null);
        return sb.ToString();
    }

    public static TextReaderBlockSkipResult ReadLine(
        this TextReaderBlockSkipState state,
        TextReader reader
    )
    {
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
            return new TextReaderBlockSkipResult(true, line);
        }
        else if (line == state.EndSkipBlock)
        {
            state.Skipping = false;
            return new TextReaderBlockSkipResult(true, line);
        }
        else if (line is null)
        {
            return new TextReaderBlockSkipResult(true, line);
        }
        else if (state.Skipping)
        {
            return new TextReaderBlockSkipResult(true, line);
        }
        else
        {
            return new TextReaderBlockSkipResult(false, line);
        }
    }
}
