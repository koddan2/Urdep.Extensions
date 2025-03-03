namespace nutool;

/// <summary>
/// An enumeration of the subcommands that can be executed by the application.
/// </summary>
public enum SubCommand
{
    /// <summary>
    /// The default, and unknown, value.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Extracts the package version data from project files.
    /// </summary>
    ExtractPackagesToCentralFile,

    /// <summary>
    /// Updates the target framework of project files.
    /// </summary>
    ChangeTargetFramework,

    /// <summary>
    /// Removes the package reference versions from project files.
    /// </summary>
    RemovePackageReferenceVersions,

    /// <summary>
    /// Sorts the package versions in MSBuild files.
    /// </summary>
    SortPackageVersions,
}
