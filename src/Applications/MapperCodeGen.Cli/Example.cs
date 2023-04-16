using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MapperCodeGen.Cli;

internal class Example
{
    private readonly ILogger _log;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHostEnvironment _hostEnvironment;

    public Example(ILoggerFactory loggerFactory, IHostEnvironment hostEnvironment)
    {
        _log = loggerFactory.CreateLogger<Example>();
        _loggerFactory = loggerFactory;
        _hostEnvironment = hostEnvironment;
    }

    internal void Run()
    {
        var contentRoot = _hostEnvironment.ContentRootPath;
        var file = Path.Combine(contentRoot, "ExampleModelType.cs");
        _log.LogInformation("Parsing file {file}", file);
        var code = File.ReadAllText(file);
        var tree = CSharpSyntaxTree.ParseText(code);

        var root = (CompilationUnitSyntax)tree.GetRoot();
        var modelCollector = new ModelCollector
        {
            Log = _loggerFactory.CreateLogger<ModelCollector>()
        };
        modelCollector.Visit(root);

        var jsonSerOpts = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(modelCollector.Models, jsonSerOpts);
        Console.WriteLine(json);

        foreach (var item in modelCollector.Models)
        {
            var codeOut = GenCode(item);
            Console.WriteLine(codeOut);
        }
    }

    private string GenCode(KeyValuePair<string, List<Dictionary<string, object>>> item)
    {
        var randBytes = new byte[4];
        RandomNumberGenerator.Fill(randBytes);
        var randSuffix = Convert.ToHexString(randBytes);
        var indent = "    ";
        var sb = new StringBuilder();
        sb.AppendLine("namespace Testing;")
            .Append(indent.Times(0))
            .AppendLine("")
            .Append(indent.Times(0))
            .Append("public static class Extensions")
            .AppendLine(randSuffix)
            .Append(indent.Times(0))
            .AppendLine("{")
            .Append(indent.Times(1))
            .Append("public static new Dictionary<string, object> ToDict(this ")
            .Append(item.Key)
            .AppendLine(" instance)")
            .Append(indent.Times(1))
            .AppendLine("{")
            .Append(indent.Times(2))
            .AppendLine("var dict = new Dictionary<string, object>();");
        foreach (var element in item.Value.Skip(1))
        {
            sb.Append(indent.Times(2))
                .Append("dict[\"")
                .Append(element["Identifier"])
                .Append("\"] = instance.")
                .Append(element["Identifier"])
                .AppendLine(";");
        }
        sb.Append(indent.Times(2)).AppendLine("return dict;");
        sb.Append(indent.Times(1)).AppendLine("}");

        sb.AppendLine();

        sb.Append(indent.Times(1))
            .Append("public static ")
            .Append(item.Key)
            .Append(" MapInto(this ")
            .Append(item.Key)
            .AppendLine(" instance, __placeholder__ fromObj)");
        sb.Append(indent.Times(1)).AppendLine("{");
        //sb.Append(indent.Times(2)).Append("var result = new ").Append(item.Key).AppendLine("();");
        sb.Append(indent.Times(2)).AppendLine("var result = instance;");
        foreach (var element in item.Value.Skip(1))
        {
            sb.Append(indent.Times(2))
                .Append("result.")
                .Append(element["Identifier"])
                .Append(" = fromObj.")
                .Append(element["Identifier"])
                .AppendLine(";");
        }
        sb.Append(indent.Times(2)).AppendLine("return result;");
        sb.Append(indent.Times(1)).AppendLine("}");

        sb.Append(indent.Times(0)).AppendLine("}");

        return sb.ToString();
    }
}

public static class Ext
{
    public static string Times(this string what, int count)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < count; ++i)
        {
            sb.Append(what);
        }
        return sb.ToString();
    }
}
