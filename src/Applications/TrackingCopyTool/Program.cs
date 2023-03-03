using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using TrackingCopyTool.Config;
using TrackingCopyTool.Utility;

namespace TrackingCopyTool;

internal class Program
{
    private static readonly Dictionary<string, string> _Stage0SwitchMappings =
        new() { ["-c"] = "ConfigurationFile" };

    private static readonly Dictionary<string, string> _Stage1SwitchMappings =
        new()
        {
            ["-v"] = "Verbosity",
            ["-d"] = "Directory",
            ["-i"] = "Includes",
            ["-x"] = "Excludes",
            ["-m"] = "ManifestFile",
        };

    // Logging level switch that will be used
    public static readonly LoggingLevelSwitch AppLoggingLevelSwitch =
        new(LogEventLevel.Information);

    private static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel
            .ControlledBy(AppLoggingLevelSwitch)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        try
        {
            if (args[0] == "install")
            {
                var assemblyLoc =
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    ?? throw new ApplicationException("Unknown");

                var target = Path.GetFullPath(args[1]);
                var targetPriv = Path.Combine(target, ProgramCfg.PrivateDirectoryName);
                InnerMain(
                    new[]
                    {
                        "-d",
                        assemblyLoc,
                        "--target:create",
                        "y",
                        "--target:name",
                        targetPriv,
                        "-v",
                        "2"
                    }
                );
            }
            else
            {
                InnerMain(args);
            }
            return 0;
        }
        catch (Exception exn)
        {
            Log.Error("Error: {msg}", exn.Message);
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static int InnerMain(string[] args)
    {
        var sw = Stopwatch.StartNew();

        var initialConfig = new ConfigurationBuilder()
            .AddCommandLine(args, _Stage0SwitchMappings)
            .Build();
        var config = new ConfigurationBuilder()
            .AddCommandLine(args, _Stage1SwitchMappings)
            .AddAllConfigurationSources(initialConfig)
            .Build();

        var cfg = new ProgramCfg(config);

        SetVerbosity(cfg);

        if (cfg.Target.Create || cfg.Force)
        {
            var path = cfg.Target.FullPath();
            Dir.Ensure(path);
        }

        Matcher matcher = new();
        matcher.AddIncludePatterns(cfg.Includes);
        matcher.AddExcludePatterns(cfg.Excludes);

        var sourceDir = cfg.SourceDirectoryFullPath;
        var targetDir = cfg.Target.FullPath();

        using var hashAlgo = Hashing.GetHashAlgorithmInstance(HashAlgo.SHA256);

        var sourceMatches = matcher.GetResultsInFullPath(sourceDir);

        Dir.Ensure(cfg.PrivateDirFullPathSource);
        Dir.Ensure(cfg.PrivateDirFullPathTarget);

        var hashes = ComputeManifest(sourceDir, sourceMatches, hashAlgo);
        WriteManifest(cfg, hashes);

        Dictionary<string, string>? targetHashes = null;
        if (File.Exists(cfg.ManifestFileFullPathTarget))
        {
            targetHashes = ReadManifest(cfg.ManifestFileFullPathTarget);
        }

        var matchesRelPaths = sourceMatches.Select(x => Path.GetRelativePath(sourceDir, x));

        var needsUpdate = false;
        long copiedBytes = 0;
        long unCopiedBytes = 0;
        foreach (var path in matchesRelPaths)
        {
            if (
                !cfg.Force
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

                var targetFileParentDir = Path.GetDirectoryName(tgt);
                if (targetFileParentDir is null)
                {
                    Log.Error("Could not determine target directory for {file}", tgt);
                    return 1;
                }

                Dir.Ensure(targetFileParentDir);

                Log.Verbose("Copying: {path}", path);
                File.Copy(src, tgt, true);
                Log.Debug("Copied: {path}", path);
                copiedBytes += new FileInfo(src).Length;
            }
        }

        if (needsUpdate)
        {
            Log.Verbose("Copying {path}", cfg.ManifestFileFullPathSource);
            File.Copy(cfg.ManifestFileFullPathSource, cfg.ManifestFileFullPathTarget, true);
            Log.Debug("Copied {path}", cfg.ManifestFileFullPathSource);
        }

        Log.Information("Copied a total of {byteCount:f2} kB", copiedBytes / 1000d);
        Log.Information("Did not copy a total of {byteCount:f2} kB", unCopiedBytes / 1000d);
        Log.Information("Normal exit - duration: {time}", sw.Elapsed);
        return 0;
    }

    private static void SetVerbosity(ProgramCfg cfg)
    {
        if (cfg.Verbosity == 0)
        {
            AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
        }
        else if (cfg.Verbosity == 1)
        {
            AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
        }
        else if (cfg.Verbosity == 2)
        {
            AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
        }
        else if (cfg.Verbosity >= 3)
        {
            AppLoggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
        }
    }

    private static Dictionary<string, string> ReadManifest(string targetManifest)
    {
        var lines = File.ReadAllLines(targetManifest);
        return lines
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x.Split(new[] { " :: " }, StringSplitOptions.TrimEntries))
            .ToDictionary(arr => arr[0], arr => arr[1]);
    }

    private static void WriteManifest(ProgramCfg cfg, Dictionary<string, string> hashes)
    {
        var path = cfg.ManifestFileFullPathSource;
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var stream = File.OpenWrite(path);
        using var sw = new StreamWriter(stream);
        foreach (var kvp in hashes)
        {
            sw.WriteLine("{0} :: {1}", kvp.Key, kvp.Value);
        }
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
