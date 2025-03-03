namespace nutool.Test;

public static class CommonTestData
{
    internal const string _PathTestOutput = "Output";
    internal const string _PathTestAssets = "TestAssets";

    internal static string _TempPath = Path.GetTempPath();
    internal static string _TempDir = Path.Combine(_TempPath, "nutool.Test");

    private static string GetPath(string path) => Path.Combine(_TempDir, path);

    internal static string PathTestOutput => GetPath(_PathTestOutput);
    internal static string PathTestAssets => GetPath(_PathTestAssets);

    internal static string GetScopedOutputDirectoryPath(string name)
    {
        var path = Path.Combine(PathTestOutput, name);
        Directory.CreateDirectory(path);
        return path;
    }

    internal static void Init()
    {
        if (Directory.Exists(_TempDir))
        {
            Directory.Delete(_TempDir, true);
        }
        Directory.CreateDirectory(_TempDir);
        CopyFilesRecursively("TestAssets", PathTestAssets);

        Ui.Out = new StringWriter();
        Ui.Err = new StringWriter();
    }

    internal static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (
            string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)
        )
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (
            string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
        )
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}
