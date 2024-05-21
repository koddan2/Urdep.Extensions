namespace CleverCopy;

internal static class ApplicationIoExtensions
{
    internal static void Log(this ApplicationIo io, VerbosityLevel verbosityLevel, string messageTemplate, params object?[]? arguments)
    {
        if (Program.Cfg.Verbosity >= verbosityLevel)
        {
            io.ErrLine(messageTemplate, arguments);
        }
    }

    internal static void Verbose(this ApplicationIo io, string messageTemplate, params object?[]? arguments)
        => io.Log(VerbosityLevel.Verbose, messageTemplate, arguments);

    internal static void Debug(this ApplicationIo io, string messageTemplate, params object?[]? arguments)
        => io.Log(VerbosityLevel.Debug, messageTemplate, arguments);

    internal static void Info(this ApplicationIo io, string messageTemplate, params object?[]? arguments)
        => io.Log(VerbosityLevel.Information, messageTemplate, arguments);

    internal static void Warn(this ApplicationIo io, string messageTemplate, params object?[]? arguments)
        => io.Log(VerbosityLevel.Warning, messageTemplate, arguments);

    internal static void Error(this ApplicationIo io, string messageTemplate, params object?[]? arguments)
        => io.Log(VerbosityLevel.Error, messageTemplate, arguments);

    internal static void Fatal(this ApplicationIo io, string messageTemplate, params object?[]? arguments)
        => io.Log(VerbosityLevel.Fatal, messageTemplate, arguments);
}
internal class ApplicationIo
{
    private readonly TextWriter _stdout = Console.Out;

    public TextWriter ErrWriter { get; } = Console.Error;

    public void Out(string msg, params object?[]? args)
        => _stdout.Write(msg, args ?? []);

    public void ErrLine(string msg, params object?[]? args)
        => ErrWriter.WriteLine(msg, args ?? []);
}