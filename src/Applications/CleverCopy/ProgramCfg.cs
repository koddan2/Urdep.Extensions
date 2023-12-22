namespace CleverCopy;

/// <summary>
/// This type describes the configuration that this program can handle.
/// </summary>
public record ProgramCfg
{
    /// <summary>
    /// Determines how verbose this program is.
    /// A setting of <c>0</c> disables all output.
    /// A setting of <c>1</c> disables all stdout output and will only write to stderr.
    /// A setting of <c>2</c> enables the most basic info on stdout (writes to stderr as usual).
    /// A setting of <c>3+</c> enables more verbose info on stdout (writes to stderr as usual).
    /// </summary>
    public int Verbosity { get; init; } = 1;

    /// <summary>
    /// If this is set to true, the program will try to create the target directory.
    /// </summary>
    public bool Force { get; init; } = false;

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
