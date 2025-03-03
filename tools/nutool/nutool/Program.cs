using nutool;

try
{
    return await Application.RunAsync(args, CancellationToken.None);
}
catch (Exception)
{
    await Ui.Err.WriteLineAsync("Unexpected error occurred.");
    throw;
}
