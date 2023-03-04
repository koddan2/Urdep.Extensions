using Microsoft.Extensions.Configuration;
using System.Diagnostics;

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
            return upper == "TRUE" || upper == "Y" || upper == "YES" || upper == "1";
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

    public static bool Bool(
        IConfiguration conf,
        string key,
        string[]? args = null,
        string? shortName = null,
        bool? isFlag = null
    )
    {
        var flagDef =
            isFlag is true && args?.Any(x => x.ToUpper() == $"--{key}" || x == shortName) is true;
        return Values.Truish(conf[key]) || flagDef;
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

internal class ProgramCfg
{
    private readonly IConfiguration _c;
    private readonly string[] _args;

    public ProgramCfg(IConfiguration c, string[] args)
    {
        _c = c;
        _args = args;
    }

    ICollection<string> DefaultIncludesTransform(ICollection<string> values)
    {
        if (values.Any())
        {
            return values.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        return new[] { "**/*.*" };
    }

    public static readonly string PrivateDirectoryName = ".TrackingCopyTool";
    public string PrivateDir => PrivateDirectoryName;
    public string PrivateDirFullPathSource => Path.Combine(SourceDirectoryFullPath, PrivateDir);
    public string PrivateDirFullPathTarget => Path.Combine(Target.FullPath(), PrivateDir);

    ICollection<string> DefaultExcludesTransform(ICollection<string> values)
    {
        return Enumerable.Concat(values, new[] { Path.Combine(PrivateDir, "**") }).ToList();
    }

    public string Directory => Required.Directory(_c, "Directory", true);

    public string SourceDirectoryFullPath =>
        Path.GetFullPath(Directory)
        ?? throw new ApplicationException($"Failed getting full path to directory: {Directory}");

    public ICollection<string> Includes => Optional.Csv(_c, "Includes", DefaultIncludesTransform);
    public ICollection<string> Excludes => Optional.Csv(_c, "Excludes", DefaultExcludesTransform);

    public bool Force => Optional.Bool(_c, "Force", _args, "-f", isFlag: true);
    public bool OnlyGenerateManifest =>
        Optional.Bool(_c, "OnlyGenerateManifest", _args, "-g", isFlag: true);

    public int Verbosity => Optional.Int(_c, "Verbosity", 0);

    public string PathHashSeparator => Optional.String(_c, "PathHashSeparator", " ¤¤ ");

    public string ManifestFile => Optional.String(_c, "ManifestFile", "manifest.txt");
    public string ManifestFileRel =>
        Path.GetRelativePath(SourceDirectoryFullPath, ManifestFileFullPathSource);
    public string ManifestFileFullPathSource =>
        Path.Combine(PrivateDirFullPathSource, ManifestFile);
    public string ManifestFileFullPathTarget =>
        Path.Combine(PrivateDirFullPathTarget, ManifestFile);
    public TargetElement Target =>
        new(Required.Directory(_c, "Target:Name", false))
        {
            Create = Optional.Bool(_c, "Target:Create"),
        };
}
