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
    public static string Directory(IConfiguration conf, string key)
    {
        var path = conf[key];
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!System.IO.Directory.Exists(path))
        {
            throw new ApplicationException($"Directory {path} does not exist.");
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

    public ProgramCfg(IConfiguration c)
    {
        _c = c;
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

    public string Directory => Required.Directory(_c, "Directory");
    public string SourceDirectoryFullPath =>
        Path.GetFullPath(Directory)
        ?? throw new ApplicationException($"Failed getting full path to directory: {Directory}");
    public ICollection<string> Includes => Optional.Csv(_c, "Includes", DefaultIncludesTransform);
    public ICollection<string> Excludes => Optional.Csv(_c, "Excludes", DefaultExcludesTransform);

    public bool Force => Get("Force", Optional.Bool);

    public int Verbosity => Optional.Int(_c, "Verbosity", 0);

    public string ManifestFile => Optional.String(_c, "ManifestFile", "manifest.txt");
    public string ManifestFileFullPathSource =>
        Path.Combine(PrivateDirFullPathSource, ManifestFile);
    public string ManifestFileFullPathTarget =>
        Path.Combine(PrivateDirFullPathTarget, ManifestFile);
    public TargetElement Target =>
        new(Get("Target:Name", Required.String)) { Create = Get("Target:Create", Optional.Bool), };

    private T Get<T>(string v, Func<IConfiguration, string, T> func)
    {
        return func(_c, v);
    }
}
