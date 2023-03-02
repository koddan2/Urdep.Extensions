using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using Serilog;
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
        new() { ["-c"] = "ConfigurationFile", };

    private static readonly Dictionary<string, string> _Stage1SwitchMappings =
        new()
        {
            ["-d"] = "Directory",
            ["-i"] = "Includes",
            ["-x"] = "Excludes",
            ["-m"] = "ManifestFile",
        };

    private static readonly ISerializer _Serializer = new SerializerBuilder().Build();
    private static readonly IDeserializer _Deserializer = new DeserializerBuilder().Build();

    private static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel
            .Debug()
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

        var programCfg = cfg.Get<ProgramCfg>();
        if (programCfg is null)
        {
            Log.Error("Could not parse configuration");
            return 1;
        }

        // to make trimming keep the ctor
        _ = new Config.TargetElement();

        if (
            (programCfg.Target?.Create is true || programCfg.Force)
            && programCfg.Target?.Name is string targetName
        )
        {
            var path = Path.GetFullPath(targetName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        var fail = false;
        foreach (var validationError in ProgramCfg.Validate(programCfg))
        {
            fail = true;
            Log.Error("Validation error: {@error}", validationError);
        }
        if (fail)
        {
            return 1;
        }

        if (!programCfg.Includes.Any())
        {
            programCfg.Includes.Add("**/*.*");
        }

        if (!programCfg.Excludes.Contains(programCfg.ManifestFile))
        {
            programCfg.Excludes.Add(programCfg.ManifestFile);
        }

        Matcher matcher = new();
        matcher.AddIncludePatterns(programCfg.Includes);
        matcher.AddExcludePatterns(programCfg.Excludes);

        var sourceDir = Path.GetFullPath(programCfg.Directory!);
        if (sourceDir == null)
        {
            Log.Error("Failed getting full path to {dir}", programCfg.Directory!);
            return 1;
        }

        using var hashAlgo = Hashing.GetHashAlgorithmInstance(HashAlgo.SHA256);

        var sourceMatches = matcher.GetResultsInFullPath(sourceDir);

        var hashes = ComputeManifest(sourceDir, sourceMatches, hashAlgo);
        WriteManifest(programCfg, sourceDir, hashes);

        var targetDir = programCfg.Target!.Name!;
        var targetManifest = Path.Combine(targetDir, programCfg.ManifestFile);
        Dictionary<string, string>? targetHashes = null;
        if (File.Exists(targetManifest))
        {
            targetHashes = ReadManifest(targetManifest);
        }

        var matchesRelPaths = sourceMatches.Select(x => Path.GetRelativePath(sourceDir, x));

        var force = programCfg.Force is true;
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
            var src = Path.Combine(sourceDir, programCfg.ManifestFile);
            var tgt = Path.Combine(targetDir, programCfg.ManifestFile);
            Log.Verbose("Copying {path}", programCfg.ManifestFile);
            File.Copy(src, tgt, true);
            Log.Debug("Copied {path}", programCfg.ManifestFile);
        }

        Log.Information("Copied a total of {byteCount:f2} kB", copiedBytes / 1000d);
        Log.Information("Did not copy a total of {byteCount:f2} kB", unCopiedBytes / 1000d);
        Log.Information("Normal exit - duration: {time}", sw.Elapsed);
        return 0;
    }

    private static Dictionary<string, string> ReadManifest(string targetManifest)
    {
        var text = File.ReadAllText(targetManifest);
        return _Deserializer.Deserialize<Dictionary<string, string>>(text);
    }

    private static void WriteManifest(
        ProgramCfg programCfg,
        string sourceDir,
        Dictionary<string, string> hashes
    )
    {
        var manifestFile = Path.Combine(sourceDir, programCfg.ManifestFile);

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
