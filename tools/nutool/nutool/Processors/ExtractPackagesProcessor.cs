using System.Xml;
using System.Xml.Linq;

namespace nutool.Processors;

internal static class ExtractPackagesProcessor
{
    internal static async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var rootPath = Application.Args.GetArgumentValue("root");
        var outfile = Application.Args.GetArgumentValue("out");

        if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(outfile))
        {
            await PrintHelpAsync();
            return 1;
        }

        if (!Directory.Exists(rootPath))
        {
            await Ui.Err.WriteLineAsync($"Directory '{rootPath}' does not exist.");
            return 1;
        }

        var csProjFiles = Utilities.GetAllPathsToCsProjFilesRecursively(rootPath);
        var packages = new HashSet<PackageVersion>();
        foreach (var csProjFile in csProjFiles)
        {
            var xml = await File.ReadAllTextAsync(csProjFile, cancellationToken);
            var xDocument = XDocument.Parse(xml);

            var packageReferences = xDocument.Descendants("PackageReference");
            foreach (var packageReference in packageReferences)
            {
                var packageName = packageReference.Attribute("Include")?.Value;
                var packageVersion = packageReference.Attribute("Version")?.Value;
                packages.Add(new PackageVersion(packageName!, packageVersion!));
            }
        }

        packages = [.. packages.OrderBy(p => p.Include, StringComparer.OrdinalIgnoreCase)];
        packages = Consolidate(packages);

        var xdoc = new XDocument();
        var rootElement = new XElement("Project");
        xdoc.Add(rootElement);

        rootElement.Add(
            new XElement("PropertyGroup", new XElement("ManagePackageVersionsCentrally", "true"))
        );

        AddPackageVersionElements(packages, rootElement);

        await using var xmlWriter = XmlWriter.Create(
            outfile,
            new XmlWriterSettings
            {
                Indent = true,
                Async = true,
                OmitXmlDeclaration = true,
            }
        );
        await xdoc.SaveAsync(xmlWriter, cancellationToken);

        return 0;
    }

    private static void AddPackageVersionElements(
        HashSet<PackageVersion> packages,
        XElement rootElement
    )
    {
        var itemGroupElement = new XElement("ItemGroup");
        rootElement.Add(itemGroupElement);

        foreach (var package in packages)
        {
            var element = new XElement("PackageVersion");
            element.SetAttributeValue("Include", package.Include);
            element.SetAttributeValue("Version", package.Version);
            itemGroupElement.Add(element);
        }
    }

    private static HashSet<PackageVersion> Consolidate(HashSet<PackageVersion> packages)
    {
        // make sure that only the latest version of each package is included
        var latestPackages = new HashSet<PackageVersion>();
        foreach (var package in packages)
        {
            if (
                latestPackages.Any(p =>
                    string.Equals(p.Include, package.Include, StringComparison.Ordinal)
                )
            )
            {
                continue;
            }

            latestPackages.Add(package);
        }

        return latestPackages;
    }

    private static async Task PrintHelpAsync()
    {
        await Ui.Err.WriteLineAsync(
            "Usage: nutool ExtractPackagesToCentralFile --root <root> --out <outfile>"
        );
    }
}
