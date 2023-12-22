using System.Linq;
using System.Reflection;
using TrackingCopyTool.Utility;

namespace CleverCopy;

/// <summary>
/// Main program class.
/// </summary>
public static class Program
{
    private static readonly string[] _HelpArgs = ["help", "?", "-h", "-?", "/h", "/?"];

    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>An integer (exit code), where zero means success, and any other value means something failed.</returns>
    public static int Main(string[] args)
    {
        try
        {
            var needHelp = args.Length == 0
                || HasAnyArg(args[0], _HelpArgs);
            if (needHelp)
            {
                PrintHelp();
                return 1;
            }

            var processor = new Processor(args);
            processor.Execute();

            return 0;
        }
        catch(ApplicationException ex)
        {
            Log.Err(ex.Message);
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
        Log.Err(File.ReadAllText(docFilePath));
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
