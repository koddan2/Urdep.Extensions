namespace nutool;

internal class ArgsParser
{
    private readonly string[] _args;

    public ArgsParser(string[] args)
    {
        _args = args;
    }

    internal string GetArgumentValue(string argName)
    {
        // find the key in the arguments
        for (var i = 0; i < _args.Length; i++)
        {
            if (string.Equals(_args[i], $"--{argName}", StringComparison.OrdinalIgnoreCase))
            {
                // return the value of the key
                return _args[i + 1];
            }
        }

        throw new NutoolException($"Argument not found: {argName}");
    }

    internal SubCommand GetSubCommand()
    {
        if (_args.Length == 0)
        {
            return default;
        }
        else if (Enum.TryParse<SubCommand>(_args[0], true, out var subCommand))
        {
            return subCommand;
        }
        else
        {
            return default;
        }
    }
}
