namespace TrackingCopyTool.Config;

internal class ProgramCfg
{
    public string? Directory { get; set; }
    public ICollection<string> Includes { get; set; } = new List<string>();
    public ICollection<string> Excludes { get; set; } = new List<string>();

    public bool Force { get; set; } = false;

    public string ManifestFile { get; set; } = ".TrackingCopyTool.manifest.yaml";
    public TargetElement? Target { get; set; }

    public static IEnumerable<string> Validate(ProgramCfg cfg)
    {
        List<string> errors = new();
        errors.AddRange(CheckDirectoryName(cfg.Directory, "-d"));
        errors.AddRange(CheckDirectoryName(cfg.Target?.Name, "--Target:Name"));
        if (string.IsNullOrEmpty(cfg.ManifestFile))
        {
            errors.Add("ManifestFile (-m) has no value.");
        }
        return errors;
    }

    private static IEnumerable<string> CheckDirectoryName(string? directory, string? argName)
    {
        List<string> errors = new();
        if (string.IsNullOrEmpty(directory))
        {
            errors.Add($"Directory ({argName}) was not supplied.");
        }
        if (!System.IO.Directory.Exists(directory))
        {
            errors.Add($"The directory {directory} does not exist.");
        }

        return errors;
    }
}
