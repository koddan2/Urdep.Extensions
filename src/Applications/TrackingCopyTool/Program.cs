using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Data.HashFunction.xxHash;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TrackingCopyTool.Config;
using TrackingCopyTool.Utility;

namespace TrackingCopyTool;

internal static class Program
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

    private static string MainModuleFileName =>
        Process.GetCurrentProcess()?.MainModule?.FileName ?? ".";

    private static int Main(string[] args)
    {
        try
        {
            if (args[0] == "install")
            {
                ////var cmdline = Environment.GetCommandLineArgs();
                var assemblyLoc =
                    ////Path.GetDirectoryName(cmdline[0])
                    Path.GetDirectoryName(MainModuleFileName) ?? Directory.GetCurrentDirectory();

                var target = Path.GetFullPath(args[1]);
                var targetPriv = Path.Combine(target, ProgramCfg.PrivateDirectoryName);
                return InnerMain(
                    [
                        "-d",
                        assemblyLoc,
                        "--target:create",
                        "y",
                        "--target:name",
                        targetPriv,
                        "-v",
                        "2"
                    ]
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
        if (!cfg.OnlyGenerateManifest && !cfg.OnlyValidateFiles)
        {
            var targetFullPath = cfg.Target.FullPath();
            Console.WriteLine("Target: {0}", targetFullPath);

            if (
                string.Equals(
                    cfg.SourceDirectoryFullPath,
                    targetFullPath,
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                Console.WriteLine("ERR: Invalid invocation: source and target are the same");
                return 1;
            }
        }

        if (!cfg.OnlyGenerateManifest && !cfg.OnlyValidateFiles && (cfg.Target.Create || cfg.Force))
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

        var hashes = cfg.UseXXH
            ? ComputeManifestXXH(sourceDir, sourceMatches)
            : ComputeManifest(sourceDir, sourceMatches, hashAlgo);
        var hashesDict = hashes.ToDictionary(x => x.Path, x => x.Hash);

        if (cfg.OnlyValidateFiles)
        {
            var readManifest = ReadManifest(cfg.ManifestFileFullPathSource, cfg);
            List<string> errors = [];
            foreach (var item in hashesDict)
            {
                if (readManifest.TryGetValue(item.Key, out string? value))
                {
                    var ok = value == item.Value;
                    if (cfg.Verbosity > 2)
                    {
                        Console.WriteLine("Hash {0} ({1})", ok ? "OK" : "Bad", item.Key);
                    }
                }
                else
                {
                    errors.Add(string.Format("Path {0} was not found in manifest", item.Key));
                }
            }

            Console.WriteLine("CMD: {0}", Environment.CommandLine);
            if (errors.Count != 0)
            {
                foreach (var error in errors)
                {
                    Console.WriteLine("ERR: {0}", error);
                }
                return 2;
            }
            else
            {
                Console.WriteLine("Validation OK, manifest: {0}", cfg.ManifestFileFullPathSource);
            }

            return 0;
        }

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
        if (
            !cfg.DisregardRestartManifest
            && !cfg.Force
            && File.Exists(cfg.RestartManifestFileFullPathTarget)
        )
        {
            targetHashes ??= [];

            var extraHashes = ReadManifest(cfg.RestartManifestFileFullPathTarget, cfg);
            foreach (var kvp in extraHashes)
            {
                if (cfg.Verbosity > 1)
                {
                    Console.WriteLine(
                        "Found hash in restart manifest: {0}={1}",
                        kvp.Key,
                        kvp.Value
                    );
                }

                //if (!targetHashes.ContainsKey(kvp.Key))
                //{
                //    targetHashes[kvp.Key] = kvp.Value;
                //}
                targetHashes[kvp.Key] = kvp.Value;
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
                && hashesDict[path] == hash
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
#pragma warning disable RCS1163 // Unused parameter.
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
#pragma warning restore RCS1163 // Unused parameter.
                Console.WriteLine();
                AppendToRestartManifest(cfg, path, hashesDict[path]);
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
            $"{100f * Math.Max(transferred, 1) / (float)Math.Max(total, 1):f2}%".PadRight(10, ' '),
            $"{total / 1000f:f2} kB".PadRight(20, ' '),
            path
        );
    }

    private static Dictionary<string, string> ReadManifest(string targetManifest, ProgramCfg cfg)
    {
        var lines = File.ReadAllLines(targetManifest).Where(x => !string.IsNullOrEmpty(x)).ToList();

        var firstLine = lines.FirstOrDefault();
        if (firstLine is not null)
        {
            var test = cfg.IsValidEntry(firstLine);
            if (!test)
            {
                Console.WriteLine(
                    $@"ERR: Manifest {targetManifest} is malformed.
ERR: Double-check that:
ERR: - it has the correct format,
ERR: - it encoded in {Encoding.Default.EncodingName},
ERR: - and the paths and hashes are separated by '{cfg.PathHashSeparator}'"
                );
                throw new ApplicationException("Could not parse the manifest file.");
            }
        }

        return lines
            .Where(cfg.IsValidEntry)
            .Select(cfg.ParseEntry)
            .ToHashSet()
            .ToDictionary(x => x.Path, x => x.Hash);
    }

    private static void WriteManifest(ProgramCfg cfg, HashSet<PathHashPair> hashes)
    {
        var path = cfg.ManifestFileFullPathSource;
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var stream = File.OpenWrite(path);
        using var sw = new StreamWriter(stream);
        foreach (var item in hashes)
        {
            var entry = cfg.MakeEntry(item.Hash, item.Path);
            sw.WriteLine("{0}", entry);
        }
    }

    private static void AppendToRestartManifest(ProgramCfg cfg, string path, string hash)
    {
        using var sw = new StreamWriter(cfg.RestartManifestFileFullPathTarget, true);
        var entry = cfg.MakeEntry(hash, path);
        sw.WriteLine("{0}", entry);
        if (cfg.Verbosity > 2)
        {
            Console.WriteLine("Appended({1}): {0}", entry, cfg.RestartManifestFileFullPathTarget);
        }
    }

    internal static HashSet<PathHashPair> ComputeManifest(
        string baseDir,
        IEnumerable<string> paths,
        HashAlgorithm hashAlgo
    )
    {
        HashSet<PathHashPair> hashes = [];
        foreach (var path in paths)
        {
            var relPath = Path.GetRelativePath(baseDir, path);
            using var stream = File.OpenRead(path);
            var hashBytes = hashAlgo.ComputeHash(stream);
            var hashStr = Convert.ToHexString(hashBytes);
            hashes.Add(new PathHashPair(hashStr, relPath));
        }

        return hashes;
    }

    public static readonly IxxHashConfig _xxHashConfig = new xxHashConfig { HashSizeInBits = 64 };
    public static readonly IxxHash _xxHash = xxHashFactory.Instance.Create(_xxHashConfig);

    internal static HashSet<PathHashPair> ComputeManifestXXH(
        string baseDir,
        IEnumerable<string> paths
    )
    {
        HashSet<PathHashPair> hashes = [];
        foreach (var path in paths)
        {
            var relPath = Path.GetRelativePath(baseDir, path);
            using var stream = File.OpenRead(path);
            var hashVal = _xxHash.ComputeHash(stream);
            var ul = BitConverter.ToUInt64(hashVal.Hash);
            var hashStr = ul.ToString(CultureInfo.InvariantCulture);
            hashes.Add(new PathHashPair(hashStr, relPath));
        }

        return hashes;
    }
}

internal record PathHashPair(string Hash, string Path);

internal static class ProgramExtensions
{
    internal static string MakeEntry(this ProgramCfg cfg, string hash, string path) =>
        $"{hash}{cfg.PathHashSeparator}{path}";

    internal static bool IsValidEntry(this ProgramCfg cfg, string entry) =>
        !string.IsNullOrEmpty(entry)
        && entry.IndexOf(cfg.PathHashSeparator) is int index
        && index < (entry.Length - 1);

    internal static PathHashPair ParseEntry(this ProgramCfg cfg, string entry)
    {
        var firstIndexOfSep = entry.IndexOf(cfg.PathHashSeparator);
        var hash = entry[..firstIndexOfSep];
        var path = entry[(firstIndexOfSep + cfg.PathHashSeparator.Length)..];
        return new PathHashPair(hash, path);
    }
}
