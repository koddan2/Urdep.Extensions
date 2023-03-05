using Microsoft.Extensions.Configuration;
using Urdep.Extensions.FileSystem;

namespace TrackingCopyTool.Config;

////internal class ConfigError
////{
////    public string? Message { get; init; }
////}
////internal class RetrievalResult<T>
////{
////    public T? Value { get; set; }
////    public ConfigError? Error { get; set; }
////}
internal static class Values
{
    internal static bool Truish(this string? v)
    {
        if (v is string s)
        {
            var upper = s.ToUpper();
            return upper == "TRUE"
                || upper == "Y"
                || upper == "YES"
                || upper == "1"
                || upper == "YE"
                || upper == "YEP";
        }
        return false;
    }
}

internal static class Optional
{
    public static ICollection<string> Csv(
        IConfiguration conf,
        string key,
        Func<ICollection<string>, ICollection<string>> transformer
    )
    {
        var val = conf[key];
        List<string> result = new();
        if (!string.IsNullOrEmpty(val))
        {
            result.AddRange(val.Split(new[] { "," }, StringSplitOptions.TrimEntries));
        }

        return transformer(result);
    }

    public static bool Bool(IConfiguration conf, string key)
    {
        return Values.Truish(conf[key]);
    }

    public static string String(IConfiguration conf, string key, string? defaultValue = null)
    {
        var val = conf[key];
        return val ?? defaultValue ?? "";
    }

    public static int Int(IConfiguration conf, string key, int? defaultValue)
    {
        return AsInt(conf[key]) ?? defaultValue ?? default;
    }

    private static int? AsInt(string? v)
    {
        if (int.TryParse(v, out int result))
        {
            return result;
        }
        return null;
    }
}

internal static class Required
{
    public static string Directory(IConfiguration conf, string key, bool mustExist = false)
    {
        var path = conf[key];
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (mustExist && !System.IO.Directory.Exists(path))
        {
            throw new ApplicationException($"Directory {path} does not exist.");
        }
        if (Path.GetFullPath(path) is null)
        {
            throw new ApplicationException($"Could not resolve full path to directory {path}");
        }

        return path;
    }

    public static string String(IConfiguration conf, string key)
    {
        return String(conf, key, null);
    }

    public static string String(IConfiguration conf, string key, string? defaultValue = null)
    {
        var val = conf[key];
        return val
            ?? defaultValue
            ?? throw new ApplicationException($"No value was supplied for {key}");
    }
}

internal record Args(string[] Arguments);

internal static class ArgsExt
{
    public static bool IsDefined(this Args args, string a)
    {
        var upper = a.ToUpper();
        for (int i = 0; i < args.Arguments.Length; i++)
        {
            if (args.Arguments[i].ToUpper() == upper)
            {
                return true;
            }
        }
        return false;
    }
}

internal class ProgramCfg
{
    private readonly IConfiguration _c;
    private readonly Args _args;

    public ProgramCfg(IConfiguration c, string[] args)
    {
        _c = c;
        _args = new Args(args);
    }

    /// <summary>
    /// The consumes the supplied includes or returns the default ones.
    /// </summary>
    /// <param name="values">The supplied values.</param>
    /// <returns>The supplied values or the default ones, if none are supplied.</returns>
    ICollection<string> DefaultIncludesTransform(ICollection<string> values)
    {
        if (values.Any())
        {
            return values.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        return new[] { "**/*.*" };
    }

    /// <summary>
    /// The name of the tool's private directory
    /// </summary>
    public static readonly string PrivateDirectoryName = ".TrackingCopyTool";

    /// <summary>
    /// Full path to the tool's private directory relative to the source directory.
    /// </summary>
    public string PrivateDirFullPathSource => Path.Combine(SourceDirectoryFullPath, PrivateDirectoryName);

    /// <summary>
    /// Full path to the tool's private directory relative to the target directory.
    /// </summary>
    public string PrivateDirFullPathTarget => Path.Combine(Target.FullPath(), PrivateDirectoryName);

    /// <summary>
    /// The transformer which makes sure the the tool's private directory is not included
    /// during normal operation.
    /// </summary>
    /// <param name="values">The supplied values.</param>
    /// <returns>The new values which excludes the tool's private directory.</returns>
    ICollection<string> DefaultExcludesTransform(ICollection<string> values)
    {
        return Enumerable.Concat(values, new[] { Path.Combine(PrivateDirectoryName, "**") }).ToList();
    }

    /// <summary>
    /// The source directory, as given by the --Directory or -d argument.
    /// </summary>
    public string Directory => Required.Directory(_c, "Directory", true);

    /// <summary>
    /// Full path to the source directory.
    /// </summary>
    public string SourceDirectoryFullPath =>
        Path.GetFullPath(Directory)
        ?? throw new ApplicationException($"Failed getting full path to directory: {Directory}");

    /// <summary>
    /// The computed, actual resulting Includes, as given by the argument --Includes (or default,
    /// which is **/*.*
    /// </summary>
    public ICollection<string> Includes => Optional.Csv(_c, "Includes", DefaultIncludesTransform);

    /// <summary>
    /// The computed, actual resulting Excludes, as given by the argument --Excludes, plus an
    /// exlude for the tool's private directory.
    /// </summary>
    public ICollection<string> Excludes => Optional.Csv(_c, "Excludes", DefaultExcludesTransform);

    /// <summary>
    /// This argument tells the tool to force create nodes on the target.
    /// </summary>
    public bool Force =>
        Optional.Bool(_c, "Force");

    /// <summary>
    /// This argument tells the tool to only generate the manifest file. Useful if the tool
    /// can be installed on the target machine and pre-generate the manifest, so that when
    /// copy operations should start, there is a ready-to-use manifest.
    /// </summary>
    public bool OnlyGenerateManifest =>
        Optional.Bool(_c, "OnlyGenerateManifest");

    /// <summary>
    /// Tells the tool to only validate the files, given the contents of the existing manifest.
    /// </summary>
    public bool OnlyValidateFiles => Optional.Bool(_c, "OnlyValidateFiles");

    /// <summary>
    /// Tells the tool to not keep track of copied files during operation. Disabling this means
    /// that the tool cannot resume a run if it is interrupted (i.e. must start from the beginning).
    /// </summary>
    public bool DisregardRestartManifest =>
        Optional.Bool(_c, "DisregardRestartManifest");

    /// <summary>
    /// This argument controls how much output the tool prints.
    /// 0 = minimal output.
    /// 1 = warnings
    /// 2 = informational
    /// 3 = debug/trace
    /// </summary>
    public int Verbosity => Optional.Int(_c, "Verbosity", 0);

    /// <summary>
    /// The separator used in the manifest to delimit the path and the hash.
    /// </summary>
    public string PathHashSeparator => Optional.String(_c, "PathHashSeparator", " ¤¤ ");

    /// <summary>
    /// The name of the manifest file.
    /// </summary>
    public string ManifestFile => Optional.String(_c, "ManifestFile", "manifest.txt");

    /// <summary>
    /// The relative path to the manifest file, with respect to the source directory.
    /// </summary>
    public string ManifestFileRel =>
        Path.GetRelativePath(SourceDirectoryFullPath, ManifestFileFullPathSource);

    /// <summary>
    /// The full path to the manifest file relative to the source directory.
    /// </summary>
    public string ManifestFileFullPathSource =>
        Path.Combine(PrivateDirFullPathSource, ManifestFile);

    /// <summary>
    /// The full path to the manifest file relative to the target directory.
    /// </summary>
    public string ManifestFileFullPathTarget =>
        Path.Combine(PrivateDirFullPathTarget, ManifestFile);

    /// <summary>
    /// The full path to the restart manifest file relative to the target directory.
    /// </summary>
    public string RestartManifestFileFullPathTarget =>
        FilenameExtensions.GetTransformedFileNameKeepParentPath(
            ManifestFileFullPathTarget,
            n => $"{n}-restart"
        );

    /// <summary>
    /// The target element, which is the name of the target, and whether to create the directory node, if not exists.
    /// </summary>
    public TargetElement Target =>
        new(Required.Directory(_c, "Target:Name", false))
        {
            Create = Optional.Bool(_c, "Target:Create"),
        };

    /// <summary>
    /// The debug element is used for debugging purposes.
    /// </summary>
    public DebugElement Debug =>
        new() { SlowerFileTransfers = Optional.Int(_c, "Debug:SlowerFileTransfers", null), };

    /// <summary>
    /// If truish, will use xxHash for hashing file contents.
    /// </summary>
    public bool UseXXH => Optional.Bool(_c, "UseXXH");
}

/// <summary>
/// Model for debug configuration.
/// </summary>
internal record DebugElement
{
    /// <summary>
    /// Slows down file transfers by sleeping the thread (in ms) for each callback from
    /// the underlying copy subroutine.
    /// </summary>
    public int? SlowerFileTransfers { get; init; }
}
