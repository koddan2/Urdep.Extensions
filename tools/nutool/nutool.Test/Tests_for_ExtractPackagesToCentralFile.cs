namespace nutool.Test;

public class Tests_for_ExtractPackagesToCentralFile
{
    [SetUp]
    public void Setup()
    {
        CommonTestData.Init();
    }

    [Test]
    public async Task Test_that_normal_invocation_works_as_expected()
    {
        var outputDir = CommonTestData.GetScopedOutputDirectoryPath(
            nameof(Tests_for_ExtractPackagesToCentralFile)
        );
        var outputFile = Path.Combine(outputDir, "result.xml");
        var args = new string[]
        {
            "ExtractPackagesToCentralFile",
            "--root",
            CommonTestData.PathTestAssets,
            "--out",
            outputFile,
        };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.EqualTo(0));

        var output = await File.ReadAllTextAsync(outputFile);
        const string expected = """
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

    [Test]
    public async Task Test_with_invalid_arguments()
    {
        var args = new string[] { "ExtractPackagesToCentralFile", "--invalidArg" };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.Not.EqualTo(0));
    }

    [Test]
    public async Task Test_with_missing_output_directory()
    {
        var args = new string[]
        {
            "ExtractPackagesToCentralFile",
            "--root",
            CommonTestData.PathTestAssets,
            "--out",
            "", // Missing output directory
        };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.Not.EqualTo(0));
    }

    [Test]
    public async Task Test_with_empty_root_directory()
    {
        var outputDir = CommonTestData.GetScopedOutputDirectoryPath(
            nameof(Tests_for_ExtractPackagesToCentralFile)
        );
        var outputFile = Path.Combine(outputDir, "result.xml");
        var args = new string[]
        {
            "ExtractPackagesToCentralFile",
            "--root",
            "", // Empty root directory
            "--out",
            outputFile,
        };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.Not.EqualTo(0));
    }

    [Test]
    public async Task Test_with_non_existent_root_directory()
    {
        var outputDir = CommonTestData.GetScopedOutputDirectoryPath(
            nameof(Tests_for_ExtractPackagesToCentralFile)
        );
        var outputFile = Path.Combine(outputDir, "result.xml");
        var args = new string[]
        {
            "ExtractPackagesToCentralFile",
            "--root",
            "non_existent_directory",
            "--out",
            outputFile,
        };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.Not.EqualTo(0));
    }
}
