using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text.Json;

namespace TrackingCopyTool.Config;

internal static class ProgramCfgExtensions
{
    public static IConfigurationBuilder AddAllConfigurationSources(
        this IConfigurationBuilder builder,
        IConfiguration stage0Conf
    )
    {
        if (stage0Conf["ConfigurationFile"] is string cfgFile)
        {
            builder.AddCfgFile(cfgFile);
        }

        var exeAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyConfigFile = Path.Combine(
            exeAssemblyDir ?? Environment.ProcessPath ?? ".",
            "appsettings"
        );
        builder.AddCfgFile(assemblyConfigFile + ".ini");
        builder.AddCfgFile(assemblyConfigFile + ".json");

        return builder;
    }

    public static IConfigurationBuilder AddCfgFile(this IConfigurationBuilder builder, string file)
    {
        if (!File.Exists(file))
        {
            return builder;
        }

        var ext = Path.GetExtension(file);
        if (ext == ".ini")
        {
            return builder.AddIniFile(file, false);
        }
        else if (ext == ".json")
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string?>>(file);
            if (dict is not null)
            {
                return builder.AddInMemoryCollection(dict.AsEnumerable());
            }
        }

        return builder;
    }

    public static string FullPath(this TargetElement targetElement)
    {
        return Path.GetFullPath(targetElement.Name)
            ?? throw new ApplicationException(
                $"Could not get full path to target:{targetElement.Name}"
            );
    }
}
