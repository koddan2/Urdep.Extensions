using System.Text;

namespace Urdep.Extensions.Text;

/// <summary>
/// Enumeration of trim mode options.
/// </summary>
public enum TrimMode
{
    /// <summary>
    /// No trimming.
    /// </summary>
    None,

    /// <summary>
    /// Trim both ends.
    /// </summary>
    Trim,

    /// <summary>
    /// Trim at the start.
    /// </summary>
    TrimStart,

    /// <summary>
    /// Trim at the end.
    /// </summary>
    TrimEnd,
}

/// <summary>
/// The state
/// </summary>
/// <param name="BeginSkipBlock">Begin skip block.</param>
/// <param name="EndSkipBlock">End skip block.</param>
/// <param name="TrimMode">Mode of trimming.</param>
public record TextReaderBlockSkipState(
    string BeginSkipBlock,
    string EndSkipBlock,
    TrimMode TrimMode = TrimMode.None
);

/// <summary>
/// Extensions.
/// </summary>
public static class TextReaderExtensions
{
    /// <summary>
    /// Reads a line.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="reader">The reader.</param>
    /// <returns>The resulting string (line).</returns>
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

    /// <summary>
    /// Reads into the supplied string builder.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="sb">The string builder.</param>
    /// <param name="reader">The reader.</param>
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
