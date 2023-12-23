namespace CleverCopy;

/// <summary>
/// Different levels of logging.
/// </summary>
public enum VerbosityLevel
{
    /// <summary>
    /// Completely silent, like a ninja.
    /// </summary>
    Silent = -1,

    /// <summary>
    /// Terminating errors or other such conditions.
    /// </summary>
    Fatal = 0,

    /// <summary>
    /// Something is wrong enough that the program will not work as intended.
    /// </summary>
    Error,

    /// <summary>
    /// The program is unsure whether something might be bad. But it might.
    /// </summary>
    Warning,

    /// <summary>
    /// Messages that should relay noteworthy content.
    /// </summary>
    Information,

    /// <summary>
    /// In order to better understand what is actually happening, without flooding the interface.
    /// </summary>
    Debug,

    /// <summary>
    /// Everything and anything.
    /// </summary>
    Verbose,
}

/// <summary>
/// This type describes the configuration that this program can handle.
/// </summary>
public record ProgramCfg
{
    /// <summary>
    /// Determines how verbose this program is.
    /// <list type="bullet">
    /// <item>A setting of <c>-1</c> disables all output.</item>
    /// <item>A setting of <c>0</c> will write only fatal errors to stderr.</item>
    /// <item>A setting of <c>1</c> will only write errors and fatal messages to stderr.</item>
    /// <item>A setting of <c>2</c> will only write warnings, errors and fatal messages to stderr.</item>
    /// <item>A setting of <c>3</c> will only write informational messages, warnings, errors and fatal messages to stderr.</item>
    /// <item>A setting of <c>4</c> will only write debug messages, informational messages, warnings, errors and fatal messages to stderr.</item>
    /// <item>A setting of <c>5+</c> enables all output to stderr.</item>
    /// </list>
    /// </summary>
    public VerbosityLevel Verbosity { get; init; } = VerbosityLevel.Information;

    /// <summary>
    /// The name of the directory in which this program stores its own data.
    /// </summary>
    public string CleverCopyDirectoryName { get; init; } = ".clevercopy";

    /// <summary>
    /// The source directory in which to enumerate files that this program shall copy to <see cref="TargetDirectory"/>.
    /// </summary>
    public required string SourceDirectory { get; init; }

    /// <summary>
    /// The optional globs to include, with respect to <see cref="SourceDirectory"/>, as supported by
    /// [Microsoft.Extensions.FileSystemGlobbing](<c>https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing</c>).
    /// If none are provided, the default globbing pattern is <c>**/*</c> - i.e. all files.
    /// </summary>
    public string[]? IncludeGlobs { get; init; }

    /// <summary>
    /// The optional globs to exclude, with respect to <see cref="SourceDirectory"/>, as supported by
    /// [Microsoft.Extensions.FileSystemGlobbing](<c>https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing</c>).
    /// If none are provided, the default is to exclude <see cref="CleverCopyDirectoryName"/> - i.e. only this program's files will be excluded.
    /// I.e. this program's files will always be excluded, regardless of what is defined in this setting.
    /// </summary>
    public string[]? ExcludeGlobs { get; init; }

    /// <summary>
    /// The target directory that this program will copy files to.
    /// </summary>
    public required string TargetDirectory { get; init; }
}
