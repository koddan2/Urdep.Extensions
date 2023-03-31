namespace TrackingCopyTool.Utility;

internal static class Dir
{
    public static void Ensure(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
