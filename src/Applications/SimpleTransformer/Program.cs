using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using Urdep.Extensions.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SimpleTransformer;

internal static class ExceptionHelper
{
    public static Exception InvalidConfiguration(
        string configurationKey,
        string message = "Unknown error"
    ) =>
        new ApplicationException(
            $"Step#{Program.step}: Invalid configuration: {configurationKey} - {message}"
        );

    internal static Exception UnknownTransform(string transform) =>
        new ApplicationException($"Step#{Program.step}: Unknown transform: {transform}");
}

internal static class Setting
{
    public static T GetOrThrow<T>(this IConfiguration instance, string key)
    {
        var result = instance.GetValue<T>(key);
        ThrowIfNullOrEmpty(result, key);
        return result!;
    }

    public static void ThrowIfNullOrEmpty(object? value, string key)
    {
        if (value is null)
        {
            throw new ApplicationException(
                $"Step#{Program.step}: The configuration key '{key}' has a null or empty value"
            );
        }
    }
}

internal static class Program
{
    internal static string? WorkingDirectory = null;
    internal static int step = 0;

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("No arguments were supplied.");
            PrintHelp();
            return 1;
        }

        WorkingDirectory = Path.GetDirectoryName(args[0]);

        var cfg = new ConfigurationBuilder().AddIniFile(args[0]).Build();

        ////var dbgView = cfg.GetDebugView();

        string? result = null;
        try
        {
            for (
                ;
                cfg.GetSection($"step{step}") is IConfigurationSection section
                    && section.GetValue<string>("transform") is string transform;
                step++
            )
            {
                Console.WriteLine("Processing step #{0}", step);
                result = await ApplyStepAsync(section, transform, result);
                // TODO remove - only for development stuff
                Console.WriteLine(result);
            }
        }
        catch (Exception)
        {
            throw;
            /* Consider
            Console.Error.WriteLine(ex.Message);
            var stackTraceFile = GetTmpFileName();
            File.WriteAllText(stackTraceFile, ex.ToString());
            Console.Error.WriteLine("Stack trace written to: {0}", stackTraceFile);
            */
        }
        return 0;
    }

    private static async Task<string?> ApplyStepAsync(
        IConfigurationSection section,
        string transform,
        string? inputValue
    )
    {
        ArgumentNullException.ThrowIfNull(section, nameof(section));
        ArgumentException.ThrowIfNullOrEmpty(transform, nameof(transform));
        if (section.GetValue<string>("source") is string source)
        {
            if (inputValue != null)
            {
                throw ExceptionHelper.InvalidConfiguration(
                    "source",
                    "result cannot be non-null when `source` is defined"
                );
            }

            var sourceFile = Path.Combine(WorkingDirectory!, source);
            inputValue = await File.ReadAllTextAsync(sourceFile);
        }
        var result = transform switch
        {
            "remove-lines" => RemoveLines(inputValue, section),
            "token-replace" => TokenReplace(inputValue, section),
            "render-simple-template" => await RenderSimpleTemplate(inputValue, section),
            _ => throw ExceptionHelper.UnknownTransform(transform),
        };

        if (section["target"] is string targetPath)
        {
            var target = Path.Combine(WorkingDirectory!, targetPath);
            await File.WriteAllTextAsync(target, result);
        }
        return result;
    }

    private static async Task<string?> RenderSimpleTemplate(
        string? result,
        IConfigurationSection section
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(result, nameof(result));
        ArgumentNullException.ThrowIfNull(section, nameof(section));
        var templateFileRelPath = section.GetOrThrow<string>("setting-template");
        var tokenToReplace = section.GetOrThrow<string>("setting-token");

        var templateFilePath = Path.Combine(WorkingDirectory!, templateFileRelPath);
        var template = await File.ReadAllTextAsync(templateFilePath);
        return template.Replace(tokenToReplace, result);
    }

    private static string? TokenReplace(string? text, IConfigurationSection section)
    {
        ArgumentException.ThrowIfNullOrEmpty(text, nameof(text));
        ArgumentNullException.ThrowIfNull(section, nameof(section));
        string tokenMapJson = section.GetOrThrow<string>("setting-token-map");
        var tokenMap =
            JsonSerializer.Deserialize<JsonObject>(tokenMapJson)
            ?? throw ExceptionHelper.InvalidConfiguration("setting-token-map", "Not valid JSON");
        foreach (KeyValuePair<string, JsonNode?> property in tokenMap)
        {
            if (property.Value is null)
            {
                throw ExceptionHelper.InvalidConfiguration(
                    $"setting-token-map:{property.Key}",
                    "Value may not be null"
                );
            }
            text = text.Replace(property.Key, property.Value.GetValue<string>());
        }
        return text;
    }

    private static string? RemoveLines(string? text, IConfigurationSection section)
    {
        ArgumentException.ThrowIfNullOrEmpty(text, nameof(text));
        ArgumentNullException.ThrowIfNull(section, nameof(section));
        var beginBlock = section.GetOrThrow<string>("setting-begin-block");
        var endBlock = section.GetOrThrow<string>("setting-end-block");

        using var reader = new StringReader(text);
        var sb = new StringBuilder();
        var state = new TextReaderBlockSkipState(beginBlock, endBlock);

        state.ReadInto(sb, reader);
        var actual = sb.ToString();
        return actual;
    }

    private static void PrintHelp()
    {
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(
            typeof(Program).Assembly.Location
        );
        Console.WriteLine("SimpleTransformer {0} ({1})", fvi.ProductVersion, fvi.FileVersion);
        Console.WriteLine(
            "Supply exactly one argument to this program, which is a path to a configuration file. A configuration file is a INI file. Take a look in the examples directory for examples."
        );
    }
}
