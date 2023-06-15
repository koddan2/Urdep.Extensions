namespace Urdep.Extensions.FileSystem;

/// <summary>
/// Extension methods for FileInfo
/// </summary>
public static class FileInfoExtensions
{
    /// <summary>
    /// Transform file name in place.
    /// </summary>
    /// <param name="fileInfo">The file info.</param>
    /// <param name="renamer">The renamer func.</param>
    /// <returns>The name of the file.</returns>
    public static string GetTransformedFileNameKeepParentPath(
        this FileInfo fileInfo,
        Func<string, string> renamer
    )
    {
        var filename = fileInfo.FullName;
        var dir = Path.GetDirectoryName(filename);
        var name = Path.GetFileNameWithoutExtension(filename);
        var ext = Path.GetExtension(filename);
        var newName = renamer(name);

        if (dir is not null)
        {
            return Path.Combine(dir, $"{newName}{ext}");
        }
        else
        {
            return $"{newName}{ext}";
        }
    }
}
