using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TrackingCopyTool.Utility;

namespace CleverCopy;

/// <summary>
/// Main program class.
/// </summary>
public static class Program
{
    private static readonly string[] _HelpArgs = ["help", "?", "-h", "-?", "/h", "/?"];

    internal static ProgramCfg Cfg = null!;

    internal static ApplicationIo Io = null!;

    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>An integer (exit code), where zero means success, and any other value means something failed.</returns>
    public static int Main(string[] args)
    {
        try
        {
            Io = new ApplicationIo();
            var needHelp = args.Length == 0
                || HasAnyArg(args[0], _HelpArgs);
            if (needHelp)
            {
                PrintHelp();
                return 1;
            }

            var cfgRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables("CLEVERCOPY_")
                .AddJsonFile("clevercopy-settings.json", optional: true)
                .AddIniFile("clevercopy-settings.ini", optional: true)
                .AddCommandLine(args)
                .Build();

            Cfg = cfgRoot.Get<ProgramCfg>()
                ?? throw Exns.GeneralError(2, "Unable to parse configuration");

            var processor = new Processor();
            processor.Execute();

            Io.Out("OK");
            return 0;
        }
        catch (ApplicationException ex)
        {
            Io.Out("FAIL");
            Io.Fatal(ex.Message);
            if (ex.Data.Contains("ErrorCode") && ex.Data["ErrorCode"] is int errorCode)
            {
                return errorCode;
            }

            return 1;
        }
    }

    private static bool HasAnyArg(string str, params string[] toCheck)
    {
        foreach (var checkStr in toCheck)
        {
            if (str.Equals(checkStr, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void PrintHelp()
    {
        var ownDir = GetDirectoryContainingThisProgram();
        var asm = GetThisProgramsAssembly();
        var attrs = asm.GetCustomAttributes();
        var assemblyProductAttr = (AssemblyProductAttribute)attrs.First(x => x.GetType().FullName == "System.Reflection.AssemblyProductAttribute");
        var docFileName = $"{assemblyProductAttr.Product}.xml";
        var docFilePath = Path.Combine(ownDir, docFileName);
        Io.ErrLine("{0}", assemblyProductAttr.Product);
        var xdoc = XDocument.Load(docFilePath);
        Io.ErrLine(xdoc.ToString());

        Io.ErrLine(@"Examples:
CleverCopy.exe --verbosity=Information \
    --sourceDirectory=c:/some/dir/with/files \
    --targetDirectory=\\remote\d$\targetdir \
    --includeGlobs:0=*.txt
    --includeGlobs:1=*.md
    --excludeGlobs:0=unimportant.txt
");
    }

    private static string GetDirectoryContainingThisProgram()
    {
        var pathToThisAssembly = GetThisProgramsAssembly().Location;
        var directoryPath = Path.GetDirectoryName(pathToThisAssembly);
        return directoryPath ?? throw Exns.GeneralError(1, "Cannot determine file system path to executable.");
    }

    private static Assembly GetThisProgramsAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
