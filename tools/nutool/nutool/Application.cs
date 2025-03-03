using nutool.Processors;

namespace nutool;

internal static class Application
{
    public static ArgsParser Args { get; private set; } = null!;

    internal static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        Args = new ArgsParser(args);

        var subCommand = Args.GetSubCommand();

        switch (subCommand)
        {
            case SubCommand.ExtractPackagesToCentralFile:
                return await ExtractPackagesProcessor.RunAsync(cancellationToken);
            default:
                await PrintHelpAsync();
                return 1;
        }
    }

    private static async Task PrintHelpAsync()
    {
        var subCommands = Enum.GetNames<SubCommand>();
        await Ui.Err.WriteLineAsync("Usage: nutool <subcommand> [options]");
        await Ui.Err.WriteLineAsync("subcommands:");
        foreach (var subCommand in subCommands)
        {
            await Ui.Err.WriteLineAsync($"  {subCommand}");
        }
    }
}
