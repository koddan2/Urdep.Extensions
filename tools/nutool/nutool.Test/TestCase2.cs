using System.Xml.Linq;

namespace nutool.Test;

public class TestCase2
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
        const string targetFramework = "net9.0";
        var args = new string[]
        {
            "ChangeTargetFramework",
            "--root",
            CommonTestData.PathTestAssets,
            "--targetframework",
            targetFramework,
        };
        var result = await Application.RunAsync(args, CancellationToken.None);
        Assert.That(result, Is.EqualTo(0));

        foreach (var csProj in _csProjs)
        {
            CheckTargetFramework(
                targetFramework,
                Path.Combine(
                    CommonTestData.PathTestAssets,
                    nameof(TestCase2),
                    csProj,
                    $"{csProj}.csproj"
                )
            );
        }
    }

    private void CheckTargetFramework(string targetFramework, string pathToCsProj)
    {
        var xdoc = XDocument.Load(pathToCsProj);

        var actual = xdoc.Descendants("TargetFramework").First().Value;
        Assert.That(actual, Is.EqualTo(targetFramework));
    }
}
