using System.Xml;
using System.Xml.Linq;

namespace nutool;

internal static class Application
{
    public static ArgsParser Args { get; private set; } = null!;

    public sealed record NugetPackage(string Name, string Version);

    internal static Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        Args = new ArgsParser(args);

        var subCommand = Args.GetSubCommand();

        switch (subCommand)
        {
            case "extract-packages":
                return ExtractPackagesAsync(cancellationToken);
            default:
                throw new NutoolException($"Unknown subcommand: {subCommand}");
        }
    }

    private static async Task<int> ExtractPackagesAsync(CancellationToken cancellationToken)
    {
        var rootPath = Args.GetArgumentValue("root");
        var outfile = Args.GetArgumentValue("out");
        var csProjFiles = Utilities.GetAllPathsToCsProjFilesRecursively(rootPath);
        var packages = new HashSet<NugetPackage>();
        foreach (var csProjFile in csProjFiles)
        {
            var xml = await File.ReadAllTextAsync(csProjFile, cancellationToken);
            var xDocument =  XDocument.Parse(xml);

            // for each PackageReference element in the xDocument file, extract the package name and version
            var packageReferences = xDocument.Descendants("PackageReference");
            foreach (var packageReference in packageReferences)
            {
                var packageName = packageReference.Attribute("Include")?.Value;
                var packageVersion = packageReference.Attribute("Version")?.Value;
                //// Console.WriteLine($"{packageName} {packageVersion}");
                packages.Add(new NugetPackage(packageName!, packageVersion!));
            }
        }

        packages = [.. packages.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)];

        var xdoc = new XDocument();
        var rootElement = new XElement("Project");
        xdoc.Add(rootElement);

        rootElement.Add(
            new XElement("PropertyGroup", new XElement("ManagePackageVersionsCentrally", "true"))
        );

        var itemGroupElement = new XElement("ItemGroup");
        rootElement.Add(itemGroupElement);

        foreach (var package in packages)
        {
            var element = new XElement("PackageVersion");
            element.SetAttributeValue("Include", package.Name);
            element.SetAttributeValue("Version", package.Version);
            itemGroupElement.Add(element);
        }

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
}
