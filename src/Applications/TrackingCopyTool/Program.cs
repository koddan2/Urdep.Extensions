using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
using TrackingCopyTool.Config;
using TrackingCopyTool.Utility;
using YamlDotNet.Serialization;

namespace TrackingCopyTool;

internal class Program
{
    private static readonly Dictionary<string, string> _Stage0SwitchMappings =
        new() { ["-c"] = "ConfigurationFile" };

    private static readonly Dictionary<string, string> _Stage1SwitchMappings =
        new()
        {
            ["-v"] = "Verbose",
            ["-d"] = "Directory",
            ["-i"] = "Includes",
            ["-x"] = "Excludes",
            ["-m"] = "ManifestFile",
        };

    // Logging level switch that will be used
    public static readonly LoggingLevelSwitch AppLoggingLevelSwitch = new LoggingLevelSwitch(
        LogEventLevel.Information
    );

    private static readonly ISerializer _Serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer _Deserializer = new DeserializerBuilder().Build();
    private static readonly string _DefaultManifestFile = ".TrackingCopyTool.manifest.yaml";

    private static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel
            .ControlledBy(AppLoggingLevelSwitch)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var sw = Stopwatch.StartNew();

        var initialCfg = new ConfigurationBuilder()
            .AddCommandLine(args, _Stage0SwitchMappings)
            .Build();
        var cfg = new ConfigurationBuilder()
            .AddCommandLine(args, _Stage1SwitchMappings)
            .AddExtraConfigurationSources(initialCfg)
            .Build();

        var dbg = cfg.GetDebugView();
        if (AsInt(cfg["Verbose"]) is int verbosity)
        {
            if (verbosity == 0)
            {
                AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
            }
            else if (verbosity == 1)
            {
                AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
            }
            else if (verbosity == 2)
            {
                AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
            }
            else if (verbosity >= 3)
            {
                AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
            }
        }

        if (
            (Truish(cfg["Target:Create"]) || Truish(cfg["Force"]))
            && cfg["Target:Name"] is string targetName
        )
        {
            var path = Path.GetFullPath(targetName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        var fail = false;
        foreach (var validationError in Validate(cfg))
        {
            fail = true;
            Log.Error("Validation error: {@error}", validationError);
        }
        if (fail)
        {
            return 1;
        }

        var includes = cfg["Includes"] ?? "";
        if (string.IsNullOrEmpty(includes))
        {
            includes = "**/*.*";
        }

        var manifestFile = cfg["ManifestFile"] ?? _DefaultManifestFile;
        var excludes = cfg["Excludes"] ?? "";
        if (!excludes.Contains(manifestFile))
        {
            excludes += $",{manifestFile}";
        }

        var includesArr = includes
            .Split(new[] { "," }, StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrEmpty(x));
        var excludesArr = excludes
            .Split(new[] { "," }, StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrEmpty(x));
        Matcher matcher = new();
        matcher.AddIncludePatterns(includesArr);
        matcher.AddExcludePatterns(excludesArr);

        var sourceDir = Path.GetFullPath(cfg["Directory"]!);
        if (sourceDir == null)
        {
            Log.Error("Failed getting full path to {dir}", cfg["Directory"]);
            return 1;
        }

        using var hashAlgo = Hashing.GetHashAlgorithmInstance(HashAlgo.SHA256);

        var sourceMatches = matcher.GetResultsInFullPath(sourceDir);

        var hashes = ComputeManifest(sourceDir, sourceMatches, hashAlgo);
        WriteManifest(manifestFile, sourceDir, hashes);

        var targetDir = cfg["Target:Name"];
        if (targetDir is null)
        {
            Log.Error("No target is defined.");
            return 1;
        }
        var targetManifest = Path.Combine(targetDir!, manifestFile);
        Dictionary<string, string>? targetHashes = null;
        if (File.Exists(targetManifest))
        {
            targetHashes = ReadManifest(targetManifest);
        }

        var matchesRelPaths = sourceMatches.Select(x => Path.GetRelativePath(sourceDir, x));

        var force = Truish(cfg["Force"]);
        var needsUpdate = false;
        long copiedBytes = 0;
        long unCopiedBytes = 0;
        foreach (var path in matchesRelPaths)
        {
            if (
                !force
                && targetHashes?.TryGetValue(path, out var hash) is true
                && hashes[path] == hash
            )
            {
                Log.Debug("SAME: {path}", path);
                unCopiedBytes += new FileInfo(Path.Combine(sourceDir, path)).Length;
            }
            else
            {
                needsUpdate = true;
                var src = Path.Combine(sourceDir, path);
                var tgt = Path.Combine(targetDir, path);

                var tgtDir = Path.GetDirectoryName(tgt);
                if (tgtDir is null)
                {
                    Log.Error("Could not determine target directory for {file}", tgt);
                    return 1;
                }
                if (!Directory.Exists(tgtDir))
                {
                    Directory.CreateDirectory(tgtDir);
                }

                Log.Verbose("Copying: {path}", path);
                File.Copy(src, tgt, true);
                Log.Debug("Copied: {path}", path);
                copiedBytes += new FileInfo(src).Length;
            }
        }

        if (needsUpdate)
        {
            var src = Path.Combine(sourceDir, manifestFile);
            var tgt = Path.Combine(targetDir, manifestFile);
            Log.Verbose("Copying {path}", manifestFile);
            File.Copy(src, tgt, true);
            Log.Debug("Copied {path}", manifestFile);
        }

        Log.Information("Copied a total of {byteCount:f2} kB", copiedBytes / 1000d);
        Log.Information("Did not copy a total of {byteCount:f2} kB", unCopiedBytes / 1000d);
        Log.Information("Normal exit - duration: {time}", sw.Elapsed);
        return 0;
    }

    private static IEnumerable<string> Validate(IConfiguration cfg)
    {
        List<string> errors = new();
        CheckDirectory(errors, cfg, "Directory");
        CheckDirectory(errors, cfg, "Target:Name");

        return errors;
    }

    private static void CheckDirectory(List<string> errors, IConfiguration cfg, string arg)
    {
        var v = cfg[arg];
        if (v is null)
        {
            errors.Add($"Argument {arg} for directory is null.");
        }

        if (!Directory.Exists(v ?? ""))
        {
            errors.Add($"Directory {v} does not exist.");
        }
    }

    private static int? AsInt(string? v)
    {
        if (int.TryParse(v, out int result))
        {
            return result;
        }
        return null;
    }

    private static bool Truish(string? v)
    {
        if (v is string s)
        {
            var upper = s.ToUpper();
            return upper == "TRUE" || upper == "Y" || upper == "YES" || upper == "1";
        }
        return false;
    }

    private static Dictionary<string, string> ReadManifest(string targetManifest)
    {
        var text = File.ReadAllText(targetManifest);
        return _Deserializer.Deserialize<Dictionary<string, string>>(text);
    }

    private static void WriteManifest(
        string manifestFilename,
        string sourceDir,
        Dictionary<string, string> hashes
    )
    {
        var manifestFile = Path.Combine(sourceDir, manifestFilename);

        var yaml = _Serializer.Serialize(hashes);
        File.WriteAllText(manifestFile, yaml);
    }

    internal static Dictionary<string, string> ComputeManifest(
        string baseDir,
        IEnumerable<string> paths,
        HashAlgorithm hashAlgo
    )
    {
        Dictionary<string, string> hashes = new();
        foreach (var path in paths)
        {
            var relPath = Path.GetRelativePath(baseDir, path);
            using var stream = File.OpenRead(path);
            var hashBytes = hashAlgo.ComputeHash(stream);
            hashes[relPath] = Convert.ToBase64String(hashBytes);
        }

        return hashes;
    }
}
