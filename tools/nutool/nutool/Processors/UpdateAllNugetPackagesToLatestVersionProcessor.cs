using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace nutool.Processors;

internal static class UpdateAllNugetPackagesToLatestVersionProcessor
{
    private const string _SourceNugetOrgV3Index = "https://api.nuget.org/v3/index.json";
    private static readonly SourceCacheContext _SourceCacheContext = new();
    private static readonly ILoggerFactory _LoggerFactory = LoggerFactory.Create(builder =>
        builder.AddConsole()
    );

    public static async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        // an MSBuild xml file, e.g. a .csproj file or Directory.packages.props
        var target = Application.Args.GetArgumentValue("target");
        if (string.IsNullOrWhiteSpace(target))
        {
            await PrintHelpAsync();
            return 1;
        }

        var xdoc = await ProcessAllPackageVersionElementsIn(target, cancellationToken);

        // save xdoc to target
        await using var xmlWriter = XmlWriter.Create(
            target,
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

    private static async Task PrintHelpAsync()
    {
        await Ui.Err.WriteLineAsync(
            "Usage: nutool UpdateAllNugetPackagesToLatestVersion [options]"
        );
    }

    private static async Task<XDocument> ProcessAllPackageVersionElementsIn(
        string target,
        CancellationToken cancellationToken
    )
    {
        var logger = _LoggerFactory.CreateLogger("");
        SourceRepository repository = Repository.Factory.GetCoreV3(_SourceNugetOrgV3Index);
        var pkgMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>(
            cancellationToken
        );

        var xml = await File.ReadAllTextAsync(target, cancellationToken);
        var xDocument = XDocument.Parse(xml);
        var versionElements = xDocument.Descendants("PackageVersion");

        var tasks = new List<Task>();
        foreach (var versionElement in versionElements)
        {
            var task = ProcessPackageVersion(
                logger,
                pkgMetadataResource,
                versionElement,
                cancellationToken
            );
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        return xDocument;
    }

    private static async Task ProcessPackageVersion(
        Microsoft.Extensions.Logging.ILogger logger,
        PackageMetadataResource pkgMetadataResource,
        XElement versionElement,
        CancellationToken cancellationToken
    )
    {
        var pkgName = versionElement.Attribute("Include")?.Value;
        if (pkgName == null)
        {
            return;
        }

        logger.LogInformation("Processing package {Package}", pkgName);

        var metadata = await pkgMetadataResource.GetMetadataAsync(
            pkgName,
            includePrerelease: false,
            includeUnlisted: false,
            _SourceCacheContext,
            new CustomLogger(logger),
            cancellationToken
        );

        var ordered = metadata.OrderBy(p => p.Identity.Version, VersionComparer.Default);
        if (ordered.Any())
        {
            var latestVersion = ordered.Last().Identity.Version;
            var versionAttr = versionElement.Attribute("Version")!;
            if (NuGetVersion.TryParse(versionAttr.Value, out var currentVersion))
            {
                if (currentVersion < latestVersion)
                {
                    versionAttr.Value = latestVersion.ToNormalizedString();
                }
                else
                {
                    logger.LogInformation(
                        "Package {Package} is already at the latest version",
                        pkgName
                    );
                }
            }
            else
            {
                logger.LogWarning(
                    "Could not parse version attribute value '{Version}' for package '{Package}'",
                    versionAttr.Value,
                    pkgName
                );
            }
        }
    }

    private sealed class CustomLogger : NuGet.Common.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _l;

        public CustomLogger(Microsoft.Extensions.Logging.ILogger logger)
        {
            _l = logger;
        }

        public void Log(NuGet.Common.LogLevel level, string data)
        {
            _l.LogInformation("{Data}", data);
        }

        public void Log(ILogMessage message)
        {
            _l.LogInformation("{Message}", message);
        }

        public Task LogAsync(NuGet.Common.LogLevel level, string data)
        {
            _l.LogInformation("{Data}", data);
            return Task.CompletedTask;
        }

        public Task LogAsync(ILogMessage message)
        {
            _l.LogInformation("{Message}", message);
            return Task.CompletedTask;
        }

        public void LogDebug(string data)
        {
            _l.LogInformation("{Data}", data);
        }

        public void LogError(string data)
        {
            _l.LogInformation("{Data}", data);
        }

        public void LogInformation(string data)
        {
            _l.LogInformation("{Data}", data);
        }

        public void LogInformationSummary(string data)
        {
            _l.LogInformation("{Data}", data);
        }

        public void LogMinimal(string data)
        {
            _l.LogInformation("{Data}", data);
        }

        public void LogVerbose(string data)
        {
            _l.LogInformation("{Data}", data);
        }

        public void LogWarning(string data)
        {
            _l.LogInformation("{Data}", data);
        }
    }
}
