using System.Xml;
using System.Xml.Linq;

namespace nutool.Processors;

internal static class ChangeTargetFrameworkProcessor
{
    internal static async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var rootPath = Application.Args.GetArgumentValue("root");
        var targetFramework = Application.Args.GetArgumentValue("targetframework");
        var csProjFiles = Utilities.GetAllPathsToCsProjFilesRecursively(rootPath);
        foreach (var csProjFile in csProjFiles)
        {
            var xml = await File.ReadAllTextAsync(csProjFile, cancellationToken);
            var xDocument = XDocument.Parse(xml);

            var targetFrameworkElement = xDocument.Descendants("TargetFramework").FirstOrDefault();
            if (targetFrameworkElement == null)
            {
                var propertyGroupElement = xDocument.Descendants("PropertyGroup").First();
                propertyGroupElement.Add(new XElement("TargetFramework", targetFramework));
            }
            else
            {
                targetFrameworkElement.Value = targetFramework;
            }

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
        }

        return 0;
    }
}
