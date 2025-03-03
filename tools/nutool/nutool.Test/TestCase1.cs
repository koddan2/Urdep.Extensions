namespace nutool.Test;

public class Tests
{
    private const string _PathTestOutput = "test-output.xml";
    private const string _PathTestAssets = "TestAssets";

    private static string _TempPath = Path.GetTempPath();
    private static string _TempDir = Path.Combine(_TempPath, "nutool.Test", "TestCase1");

    private static string PathTestOutput => GetPath(_PathTestOutput);
    private static string PathTestAssets => GetPath(_PathTestAssets);

    private static string GetPath(string path) => Path.Combine(_TempDir, path);

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
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

    [SetUp]
    public void Setup()
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

    [Test]
    public async Task Test1()
    {
        var args = new string[]
        {
            "ExtractPackagesToCentralFile",
            "--root",
            PathTestAssets,
            "--out",
            PathTestOutput,
        };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.EqualTo(0));

        var output = await File.ReadAllTextAsync(PathTestOutput);
        var expected = """
            <Project>
              <PropertyGroup>
                <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
              </PropertyGroup>
              <ItemGroup>
                <PackageVersion Include="Meziantou.Analyzer" Version="2.0.188" />
                <PackageVersion Include="Pkg1" Version="1.0.1" />
                <PackageVersion Include="Roslynator.Core" Version="4.13.1" />
                <PackageVersion Include="SonarAnalyzer.CSharp" Version="10.7.0.110445" />
              </ItemGroup>
            </Project>
            """;
        Assert.That(output, Is.EqualTo(expected));
    }
}
