using System.Collections.Generic;
using TrackingCopyTool.Utility;

namespace CleverCopy;

internal class Manifest
{
    internal static readonly string ManifestFileName = "manifest.yaml";

    public Manifest() { }
    public Manifest(string baseDir, IEnumerable<string> absolutePathsToFiles, bool loadExistingManifestOnly = false)
    {
        Timestamp = DateTimeOffset.Now;
        IsLittleEndian = BitConverter.IsLittleEndian;

        if (loadExistingManifestOnly)
        {
            var privateDir = Path.Combine(baseDir, Program.Cfg.CleverCopyDirectoryName);
            var fileToRead = Path.Combine(privateDir, ManifestFileName);
            if (File.Exists(fileToRead))
            {
                var tempManifest = Manifest.ReadFromFileSystem(privateDir);
                Hashes = tempManifest.Hashes;
            }
        }
        else
        {
            foreach (var absPathToFile in absolutePathsToFiles)
            {
                var bytes = File.ReadAllBytes(absPathToFile);
                var hashVal = System.IO.Hashing.XxHash3.HashToUInt64(bytes);
                var relPath = Path.GetRelativePath(baseDir, absPathToFile);
                Hashes[relPath] = hashVal;
            }
        }
    }

    public DateTimeOffset Timestamp { get; init; }
    public bool IsLittleEndian { get; init; }
    public Dictionary<string, ulong> Hashes { get; init; } = [];

    internal void WriteToFileSystem(string containingDir)
    {
        var file = Path.Combine(containingDir, ManifestFileName);
        Program.Io.Verbose("Writing manifest to file: {0}", file);
        YamlHelper.SerializeToFile(file, this);
        Program.Io.Debug("Manifest written to file: {0}", file);
    }

    internal static Manifest ReadFromFileSystem(string containingDir)
    {
        var file = Path.Combine(containingDir, ManifestFileName);
        Program.Io.Info("Reading manifest file: {0}", file);
        var result = YamlHelper.DeserializeFromFile<Manifest>(file)
            ?? throw Exns.GeneralError(4, "Unable to read manifest file at {0}", file);
        Program.Io.Verbose("Manifest read from file: {0}", file);
        return result;
    }
}
