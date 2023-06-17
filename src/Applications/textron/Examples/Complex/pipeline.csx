#r "nuget: SimpleExec, 11.0.0"
#r "nuget: System.Console, 4.3.1"
#r "nuget: Bullseye, 4.2.1"
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static Bullseye.Targets;
using static SimpleExec.Command;
Console.WriteLine(Environment.CurrentDirectory);
Console.WriteLine(JsonSerializer.Serialize(args));

Stream? input = null;
string workspace = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "workspace"));
string WorkspaceFile(string name) => Path.Combine(workspace, name);

int step = 0;
void Step(Action target)
{
    if (step is 0)
    {
        Target($"step{step}", target);
    }
    else
    {
        Target($"step{step}", DependsOn($"step{step - 1}"), target);
    }
    step += 1;
}
string LastStep() => $"step{step - 1}";

Step(() =>
{
    if (Directory.Exists(workspace)) { Directory.Delete(workspace, true); }
    Directory.CreateDirectory(workspace);
});
Step(() => input = File.OpenRead("source-file.sql"));
Step(() =>
{
    using var reader = new StreamReader(input);
    using var outputStream = File.OpenWrite(WorkspaceFile("step1.sql"));
    using var output = new StreamWriter(outputStream);
    var skip = false;
    while (reader.ReadLine() is string line)
    {
        if (line.StartsWith("--[[INTERACTIVE"))
        {
            skip = true;
            continue;
        }
        else if (line.StartsWith("--]]"))
        {
            skip = false;
            continue;
        }

        if (skip || string.IsNullOrWhiteSpace(line)) continue;
        else output.WriteLine(line);
    }
});
Step(() =>
{
    using var reader = new StreamReader(WorkspaceFile("step1.sql"));
    using var outputStream = File.OpenWrite(WorkspaceFile("step2.sql"));
    using var output = new StreamWriter(outputStream);
    while (reader.ReadLine() is string line)
    {
        var updatedLine = line
            .Replace("REPLACE-WITH($env:SystemDrive)", Environment.GetEnvironmentVariable("SystemDrive"))
            .Replace("/*REPLACE-WITH(@p0)*/", "@p0");
        if (string.IsNullOrWhiteSpace(updatedLine)) continue;
        else output.WriteLine(updatedLine);
    }
});
Step(() =>
{
    var template = File.ReadAllText("template.xml");
    var content = File.ReadAllText(WorkspaceFile("step2.sql"));
    var result = template.Replace("RENDER-HERE", content);
    File.WriteAllText(WorkspaceFile("result.xml"), result);
});
Target("default", DependsOn(LastStep()));
await RunTargetsAndExitAsync(new string[]{"--no-color"}, ex => ex is SimpleExec.ExitCodeException);