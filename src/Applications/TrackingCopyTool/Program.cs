using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TrackingCopyTool.Config;
using TrackingCopyTool.Utility;
using Urdep.Extensions.FileSystem;

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

    private static ProgramCfg? _Cfg;

    private static int Main(string[] args)
    {
        try
        {
            if (args[0] == "install")
            {
                var cmdline = Environment.GetCommandLineArgs();

                var assemblyLoc =
                    Path.GetDirectoryName(cmdline[0])
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
            if (_Cfg is null || _Cfg.Verbosity > 2)
            {
                Console.WriteLine(exn.StackTrace);
            }
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
        _Cfg = cfg;

        if (cfg.Verbosity > 2)
        {
            Console.WriteLine(config.GetDebugView());
        }

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
        if (!cfg.Force && File.Exists(cfg.ManifestFileFullPathTarget))
        {
            targetHashes = ReadManifest(cfg.ManifestFileFullPathTarget, cfg);
        }
        if (!cfg.DisregardRestartManifest && !cfg.Force && File.Exists(cfg.RestartManifestFileFullPathTarget))
        {
            targetHashes ??= new Dictionary<string, string>();

            var extraHashes = ReadManifest(cfg.RestartManifestFileFullPathTarget, cfg);
            foreach (var kvp in extraHashes)
            {
                if (cfg.Verbosity > 2)
                {
                    Console.WriteLine("Found hash in restart manifest: {0}={1}", kvp.Key, kvp.Value);
                }

                if (!targetHashes.ContainsKey(kvp.Key))
                {
                    targetHashes[kvp.Key] = kvp.Value;
                }
            }
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

                        if (cfg.Debug.SlowerFileTransfers is int slower)
                        {
                            Thread.Sleep(slower);
                        }

                        return FileCopyHelper.CopyProgressResult.PROGRESS_CONTINUE;
                    }
                );
                Console.WriteLine();
                AppendToRestartManifest(cfg, path, hashes[path]);
                copiedBytes += new FileInfo(src).Length;
            }
        }

        if (needsUpdate)
        {
            File.Copy(cfg.ManifestFileFullPathSource, cfg.ManifestFileFullPathTarget, true);
            var fi = new FileInfo(cfg.ManifestFileFullPathSource);
            PrintProgress(fi.Length, fi.Length, fi.FullName);
            Console.WriteLine();
        }

        // clean up: delete restart manifest.
        if (File.Exists(cfg.RestartManifestFileFullPathTarget))
        {
            if (cfg.Verbosity > 2)
            {
                Console.WriteLine("Deleting {0}", cfg.RestartManifestFileFullPathTarget);
            }
            File.Delete(cfg.RestartManifestFileFullPathTarget);
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
        var lines = File.ReadAllLines(targetManifest)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList()
            ;

        var firstLine = lines.FirstOrDefault();
        if (firstLine is not null)
        {
            var test = firstLine.Split(new[] { cfg.PathHashSeparator }, StringSplitOptions.TrimEntries);
            if (test.Length != 2)
            {
                Console.WriteLine($"ERR: Manifest {targetManifest} is malformed.");
                Console.WriteLine($"ERR: Double-check that:");
                Console.WriteLine($"ERR: - it has the correct format,");
                Console.WriteLine($"ERR: - it encoded in {Encoding.Default.EncodingName},");
                Console.WriteLine($"ERR: - and the paths and hashes are separated by '{cfg.PathHashSeparator}'");
                throw new ApplicationException("Could not parse the manifest file.");
            }
        }

        return lines
            .Select(x => x.Split(new[] { cfg.PathHashSeparator }, StringSplitOptions.TrimEntries))
            .Where(x => x.Length == 2)
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

    private static void AppendToRestartManifest(ProgramCfg cfg, string path, string hash)
    {
        using var sw = new StreamWriter(cfg.RestartManifestFileFullPathTarget, true);
        sw.WriteLine("{0}{1}{2}", path, cfg.PathHashSeparator, hash);
        if (cfg.Verbosity > 2)
        {
            Console.WriteLine("Appended({3}): {0}{1}{2}", path, cfg.PathHashSeparator, hash, cfg.RestartManifestFileFullPathTarget);
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
