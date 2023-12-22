using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.Text.Json;
using TrackingCopyTool.Utility;

namespace CleverCopy;

internal class Manifest
{
    private static readonly JsonSerializerOptions _JsonSerializerOptions = new() { WriteIndented = true };
    public Manifest(ProgramCfg cfg, IEnumerable<string> absolutePathsToFiles)
    {
        IsLittleEndian = BitConverter.IsLittleEndian;

        foreach (var absPathToFile in absolutePathsToFiles)
        {
            var bytes = File.ReadAllBytes(absPathToFile);
            var hashVal = System.IO.Hashing.XxHash3.HashToUInt64(bytes);
            var relPath = Path.GetRelativePath(cfg.SourceDirectory, absPathToFile);
            Hashes[relPath] = hashVal;
        }
    }

    public Dictionary<string, ulong> Hashes { get; } = [];
    public bool IsLittleEndian { get; }

    internal void WriteStateToFileSystem(string containingDir)
    {
        var json = JsonSerializer.Serialize(this, _JsonSerializerOptions);
        var file = Path.Combine(containingDir, "manifest.json");
        File.WriteAllText(file, json);
    }
}
internal class Processor
{
    private readonly IConfigurationRoot _cfgRoot;
    private readonly ProgramCfg _cfg;
    private readonly Stopwatch _stopwatch;
    private readonly IEnumerable<string> _filesAbsolutePaths;
    private readonly Manifest _manifestSource;

    public Processor(string[] args)
    {
        _stopwatch = Stopwatch.StartNew();
        _stopwatch.Start();

        _cfgRoot = new ConfigurationBuilder()
            .AddEnvironmentVariables("CLEVERCOPY_")
            .AddJsonFile("clevercopy-settings.json", optional: true)
            .AddIniFile("clevercopy-settings.ini", optional: true)
            .AddCommandLine(args)
            .Build();

        _cfg = _cfgRoot.Get<ProgramCfg>()
            ?? throw Exns.GeneralError(2, "Unable to parse configuration");

        if (!Directory.Exists(_cfg.SourceDirectory))
        {
            throw Exns.GeneralError(3, $"The directory {_cfg.SourceDirectory} ({nameof(_cfg.SourceDirectory)}) does not exist.");
        }

        Matcher matcher = new(StringComparison.OrdinalIgnoreCase);
        matcher.AddIncludePatterns(_cfg.GetIncludeGlobs());
        matcher.AddExcludePatterns(_cfg.GetExcludeGlobs());

        _filesAbsolutePaths = matcher.GetResultsInFullPath(_cfg.SourceDirectory);

        _manifestSource = new Manifest(_cfg, _filesAbsolutePaths);
    }

    public void Execute()
    {
        Step0Prepare();
    }

    private void Step0Prepare()
    {
        Directory.CreateDirectory(_cfg.GetSourceDirectoryCleverCopyDirectory());
        _manifestSource.WriteStateToFileSystem(_cfg.GetSourceDirectoryCleverCopyDirectory());
    }
}