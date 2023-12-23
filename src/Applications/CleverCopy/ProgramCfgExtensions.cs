namespace CleverCopy;

internal static class ProgramCfgExtensions
{
    public static string[] GetIncludeGlobs(this ProgramCfg cfg)
    {
        if (cfg.IncludeGlobs is null || cfg.IncludeGlobs.Length == 0)
        {
            return ["**/*"];
        }

        return cfg.IncludeGlobs;
    }

    public static string[] GetExcludeGlobs(this ProgramCfg cfg)
    {
        string[] defaultExcludeGlobs = [cfg.CleverCopyDirectoryName];
        if (cfg.ExcludeGlobs is null || cfg.ExcludeGlobs.Length == 0)
        {
            return defaultExcludeGlobs;
        }

        return [.. cfg.ExcludeGlobs, .. defaultExcludeGlobs];
    }

    public static string GetSourceDirectoryCleverCopyDirectory(this ProgramCfg cfg)
        => Path.Combine(cfg.SourceDirectory, cfg.CleverCopyDirectoryName);

    public static string GetTargetDirectoryCleverCopyDirectory(this ProgramCfg cfg)
        => Path.Combine(cfg.TargetDirectory, cfg.CleverCopyDirectoryName);
}
