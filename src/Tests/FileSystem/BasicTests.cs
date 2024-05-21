using Urdep.Extensions.FileSystem;

namespace Tests.FileSystem;

internal class BasicTests
{
    [Test]
    public void Test_WalkUpUntil_1()
    {
        var baseDir = AppContext.BaseDirectory;
        var di = new DirectoryInfo(baseDir);
        var somwhere = di.WalkUpUntil(
            x => x.EnumerateFiles().Select(fi => fi.Name).Contains("Tests.csproj")
        );
        Assert.That(somwhere, Is.Not.Null);

        // .../bin/Debug/net7.0
        var target = Path.GetFullPath(Path.Combine(baseDir, "../../.."));
        Assert.That(somwhere!.FullName, Is.EqualTo(target));
    }
}
