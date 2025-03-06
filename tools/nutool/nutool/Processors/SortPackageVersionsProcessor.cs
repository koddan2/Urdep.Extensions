using System.Xml.Linq;

namespace nutool.Processors;

internal static class SortPackageVersionsProcessor
{
    internal static async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var targetfile = Application.Args.GetArgumentValue("target");
        if (string.IsNullOrWhiteSpace(targetfile))
        {
            await PrintHelpAsync();
            return 1;
        }

        // load targetfile and treat it as a msbuild file.
        // we shall process each ItemGroup element that contains PackageVersion elements
        // we will sort the PackageVersion elements by Include attribute
        await ProcessAllPackageVersionElementsIn(targetfile, cancellationToken);
        await Ui.Out.WriteLineAsync($"Sorted package versions in {targetfile}");
        return 0;
    }

    private static async Task PrintHelpAsync()
    {
        await Ui.Err.WriteLineAsync("Usage: nutool SortPackageVersions [options]");
        await Ui.Err.WriteLineAsync("Options:");
        await Ui.Err.WriteLineAsync("   --target  The target file to operate on.");
    }

    private static async Task ProcessAllPackageVersionElementsIn(
        string targetfile,
        CancellationToken cancellationToken
    )
    {
        XDocument xdoc = await LoadXmlAsync(targetfile, cancellationToken);
        // Process xdoc here
        var itemGroups = xdoc.Descendants("ItemGroup");
        HashSet<string> allPackageVersionsIncludes = new(StringComparer.OrdinalIgnoreCase);
        foreach (var itemGroup in itemGroups)
        {
            var packageVersions = itemGroup.Descendants("PackageVersion");
            var sortedPackageVersions = packageVersions
                .OrderBy(p => p.Attribute("Include")?.Value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            packageVersions.Remove();
            ////itemGroup.Add(sortedPackageVersions);
            // loop over all package versions and add them to allPackageVersionsIncludes but check
            // if exists first and report error if already exists
            foreach (var packageVersion in sortedPackageVersions)
            {
                itemGroup.Add(packageVersion);
                var include = packageVersion.Attribute("Include")?.Value;
                if (allPackageVersionsIncludes.Contains(include ?? ""))
                {
                    await Ui.Err.WriteLineAsync(
                        $"Duplicate package version '{include}' found in {targetfile}"
                    );
                }
                allPackageVersionsIncludes.Add(include ?? "");
            }
        }

        var xml = xdoc.ToString();
        xdoc = XDocument.Parse(xml);

        await File.WriteAllTextAsync(targetfile, xdoc.ToString());
    }

    private static async Task<XDocument> LoadXmlAsync(
        string targetfile,
        CancellationToken cancellationToken
    )
    {
        await using var stream = File.OpenRead(targetfile);
        var xdoc = await XDocument.LoadAsync(
            stream,
            LoadOptions.PreserveWhitespace,
            cancellationToken
        );
        ////var text = await File.ReadAllTextAsync(targetfile, cancellationToken);
        ////var xdoc = XDocument.Parse(text);
        return xdoc;
    }
}
