using Microsoft.Extensions.Configuration;

namespace TrackingCopyTool.Config;

internal static class ProgramCfgExtensions
{
    public static IConfigurationBuilder AddExtraConfigurationSources(
        this IConfigurationBuilder builder,
        IConfiguration stage0Conf
    )
    {
        if (stage0Conf["ConfigurationFile"] is string cfgFile)
        {
            builder.AddIniFile(cfgFile);
        }
        return builder;
    }
}
