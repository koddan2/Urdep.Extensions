using textron.Engine;

namespace textron;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        string[] helpArgs =
        [
            "help", "?", "-h", "--help"
        ];
        if (args.Length != 1)
        {
            PrintHelp();
            return 1;
        }
        else if (helpArgs.Contains(args[0]))
        {
            PrintHelp();
            return 0;
        }

        try
        {
            ITransformerPipeline pipeline = new XmlBasedTransformerPipeline(args[0]);
            await pipeline.ProcessAsync();
        }
        catch (Exception exn)
        {
            Console.Error.WriteLine(exn.Message);
            return 1;
        }

        return 0;
    }

    private static void PrintHelp()
    {
        var assembly = typeof(Program).Assembly;
        var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(
            assembly.Location
        );
        Console.WriteLine("{0} - version {1}", assembly.GetName().Name, fileVersionInfo.ProductVersion);
        Console.WriteLine(
            "Supply exactly one argument to this program, which is a path to a pipeline XML file."
        );
    }
}
