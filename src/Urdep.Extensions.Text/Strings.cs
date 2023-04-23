namespace Urdep.Extensions.Text;

/// <summary>
/// Helper stuff related to strings.
/// </summary>
public static class Strings
{
    /// <summary>
    /// Get a lazy sequence of the lines of the supplied input.
    /// </summary>
    /// <param name="input">The possibly multi-line string.</param>
    /// <returns>The sequence of substrings delimited by \r\n or \n</returns>
    public static IEnumerable<string> GetLines(string input)
    {
        if (input == null)
        {
            yield break;
        }

        using StringReader reader = new(input);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }
}
