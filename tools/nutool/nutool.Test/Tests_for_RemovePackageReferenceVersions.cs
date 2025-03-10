using System.Xml.Linq;

namespace nutool.Test;

public class Tests_for_RemovePackageReferenceVersions
{
    [SetUp]
    public void Setup()
    {
        CommonTestData.Init();
    }

    private readonly HashSet<string> _csProjs = ["ProjA", "ProjB"];

    [Test]
    public async Task Test_that_normal_invocation_works_as_expected()
    {
        var args = new string[]
        {
            "RemovePackageReferenceVersions",
            "--root",
            CommonTestData.PathTestAssets,
        };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.EqualTo(0));

        foreach (var csProj in _csProjs)
        {
            CheckCsProjFilePackageReferencesAreWithoutVersionAttribute(
                Path.Combine(
                    CommonTestData.PathTestAssets,
                    nameof(Tests_for_RemovePackageReferenceVersions),
                    csProj,
                    $"{csProj}.csproj"
                )
            );
        }
    }

    private static void CheckCsProjFilePackageReferencesAreWithoutVersionAttribute(
        string csProjFile
    )
    {
        // load the xml and check all packageReference elements
        var xml = File.ReadAllText(csProjFile);
        var xDocument = XDocument.Parse(xml);
        var itemGroups = xDocument.Descendants("ItemGroup");
        foreach (var itemGroup in itemGroups)
        {
            var packageReferences = itemGroup.Descendants("PackageReference");
            foreach (var packageReference in packageReferences)
            {
                Assert.That(packageReference.Attribute("Version"), Is.Null);
            }
        }
    }
}
