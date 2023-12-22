namespace CleverCopy;

internal static class Log
{
    public static void Inf(string msg, params object?[]? args)
    {
        Console.WriteLine(msg, args);
    }

    public static void Err(string msg, params object?[]? args)
    {
        Console.Error.WriteLine(msg, args ?? Array.Empty<string>());
    }
}