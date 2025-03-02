// See https://aka.ms/new-console-template for more information
try
{
    await nutool.Application.RunAsync(args, CancellationToken.None);
}
catch (Exception)
{
    await Console.Error.WriteLineAsync("Unexpected error occurred.");
    throw;
}
