using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
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
            ["-g"] = "OnlyGenerateManifest",
        };

    private static int Main(string[] args)
    {
        try
        {
            if (args[0] == "install")
            {
                var assemblyLoc =
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    ?? Path.GetFullPath(".");

                var target = Path.GetFullPath(args[1]);
                var targetPriv = Path.Combine(target, ProgramCfg.PrivateDirectoryName);
                return InnerMain(
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
                return InnerMain(args);
            }
        }
        catch (Exception exn)
        {
            Console.WriteLine("ERR: {0}", exn.Message);
            return 1;
        }
        ////finally
        ////{
        ////}
    }

    private static int InnerMain(string[] args)
    {
        var startTime = DateTimeOffset.Now;
        Console.WriteLine("Started at {0}", startTime);
        var sw = Stopwatch.StartNew();

        var initialConfig = new ConfigurationBuilder()
            .AddCommandLine(args, _Stage0SwitchMappings)
            .Build();
        var config = new ConfigurationBuilder()
            .AddCommandLine(args, _Stage1SwitchMappings)
            .AddAllConfigurationSources(initialConfig)
            .Build();

        var cfg = new ProgramCfg(config, args);

        Console.WriteLine("Source: {0}", cfg.SourceDirectoryFullPath);
        if (!cfg.OnlyGenerateManifest)
        {
            Console.WriteLine("Target: {0}", cfg.Target.FullPath());
        }

        if (!cfg.OnlyGenerateManifest && (cfg.Target.Create || cfg.Force))
        {
            var path = cfg.Target.FullPath();
            Dir.Ensure(path);
        }

        Matcher matcher = new();
        matcher.AddIncludePatterns(cfg.Includes);
        matcher.AddExcludePatterns(cfg.Excludes);

        var sourceDir = cfg.SourceDirectoryFullPath;

        using var hashAlgo = Hashing.GetHashAlgorithmInstance(HashAlgo.SHA256);

        var sourceMatches = matcher.GetResultsInFullPath(sourceDir);

        Dir.Ensure(cfg.PrivateDirFullPathSource);

        var hashes = ComputeManifest(sourceDir, sourceMatches, hashAlgo);
        WriteManifest(cfg, hashes);

        if (cfg.OnlyGenerateManifest)
        {
            var outfile = cfg.ManifestFileFullPathSource;
            Console.WriteLine("Manifest generated in {0}: {1} - exiting.", sw.Elapsed, outfile);
            return 0;
        }

        var targetDir = cfg.Target.FullPath();
        Dir.Ensure(cfg.PrivateDirFullPathTarget);

        Dictionary<string, string>? targetHashes = null;
        if (File.Exists(cfg.ManifestFileFullPathTarget))
        {
            targetHashes = ReadManifest(cfg.ManifestFileFullPathTarget, cfg);
        }

        var matchesRelPaths = sourceMatches.Select(x => Path.GetRelativePath(sourceDir, x));

        var needsUpdate = false;
        long copiedBytes = 0;
        long unCopiedBytes = 0;
        foreach (var path in matchesRelPaths)
        {
            // if crash
            ////using var writer = new StreamWriter(cfg.TemporaryManifestFileFullPathTarget, true);
            if (
                !cfg.Force
                && targetHashes?.TryGetValue(path, out var hash) is true
                && hashes[path] == hash
            )
            {
                Console.WriteLine("SAME: {0}", path);
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
                    Console.WriteLine("Could not determine target directory for {0}", tgt);
                    return 1;
                }

                Dir.Ensure(targetFileParentDir);

                ////Console.WriteLine("COPY: {0}", path);
                ////File.Copy(src, tgt, true);
                new FileCopyHelper().XCopyEz(
                    src,
                    tgt,
                    (total, transferred, streamSize, reason) =>
                    {
                        PrintProgress(total, transferred, path);
                        return FileCopyHelper.CopyProgressResult.PROGRESS_CONTINUE;
                    }
                );
                Console.WriteLine();
                ////Log.Debug("CP ✓: {path}", path);
                copiedBytes += new FileInfo(src).Length;
            }
        }

        if (needsUpdate)
        {
            ////Console.WriteLine("COPY: {0}", cfg.ManifestFileRel);
            File.Copy(cfg.ManifestFileFullPathSource, cfg.ManifestFileFullPathTarget, true);
            var fi = new FileInfo(cfg.ManifestFileFullPathSource);
            PrintProgress(fi.Length, fi.Length, fi.FullName);
            Console.WriteLine();
        }

        Console.WriteLine("Copied a total of:        {0:f2} kB", copiedBytes / 1000d);
        Console.WriteLine("Did not copy a total of:  {0:f2} kB", unCopiedBytes / 1000d);
        Console.WriteLine("Duration:                 {0}", sw.Elapsed);
        Console.WriteLine("Started at:               {0}", startTime.ToString());
        Console.WriteLine("Ended at:                 {0}", DateTimeOffset.Now.ToString());
        Console.WriteLine("Normal exit (0)");
        return 0;
    }

    private static void PrintProgress(long total, long transferred, string path)
    {
        Console.Write(
            "\r{0} {1} {2}",
            $"{(100f * transferred / (float)total):f2}%".PadRight(10, ' '),
            $"{(total / 1000f):f2} kB".PadRight(20, ' '),
            path
        );
    }

    private static Dictionary<string, string> ReadManifest(string targetManifest, ProgramCfg cfg)
    {
        var lines = File.ReadAllLines(targetManifest);
        return lines
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x.Split(new[] { cfg.PathHashSeparator }, StringSplitOptions.TrimEntries))
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
            sw.WriteLine("{0}{1}{2}", kvp.Key, cfg.PathHashSeparator, kvp.Value);
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
