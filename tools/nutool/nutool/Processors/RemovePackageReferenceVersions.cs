using System.Xml;
using System.Xml.Linq;

namespace nutool.Processors;

internal static class RemovePackageReferenceVersions
{
    internal static async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var rootPath = Application.Args.GetArgumentValue("root");
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            await PrintHelpAsync();
            return 1;
        }

        var csProjFiles = Utilities.GetAllPathsToCsProjFilesRecursively(rootPath);
        foreach (var csProjFile in csProjFiles)
        {
            await ProcessCsProjFile(csProjFile, cancellationToken);
        }

        return 0;
    }

    private static async Task ProcessCsProjFile(
        string csProjFile,
        CancellationToken cancellationToken
    )
    {
        // load the csproj file
        var xml = await File.ReadAllTextAsync(csProjFile, cancellationToken);
        var xDocument = XDocument.Parse(xml);

        // for each ItemGroup element that contains PackageReference elements, remove the Version attribute
        var itemGroups = xDocument.Descendants("ItemGroup");
        foreach (var itemGroup in itemGroups)
        {
            var packageReferences = itemGroup.Descendants("PackageReference");
            foreach (var packageReference in packageReferences)
            {
                packageReference.Attribute("Version")?.Remove();
            }
        }

        // write the xml back to the file
        await using var xmlWriter = XmlWriter.Create(
            csProjFile,
            new XmlWriterSettings
            {
                Indent = true,
                Async = true,
                OmitXmlDeclaration = true,
            }
        );
        await xDocument.SaveAsync(xmlWriter, cancellationToken);
        await Ui.Out.WriteLineAsync($"Updated {csProjFile}");
    }

    private static async Task PrintHelpAsync()
    {
        await Ui.Err.WriteLineAsync("Usage: nutool RemovePackageReferenceVersions [options]");
        await Ui.Err.WriteLineAsync("Options:");
        await Ui.Err.WriteLineAsync("   --root  The root directory to operate on.");
    }
}
