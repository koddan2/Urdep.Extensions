using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.XPath;

namespace textron.Engine;

public interface ITransformerPipeline
{
    Task ProcessAsync();
}
public static class ValidationExtensions
{
    [return: NotNull]
    public static T Require<T>(this T? reference, string? message = null)
    {
        if (reference is null)
        {
            throw new ArgumentNullException(nameof(reference) + message is null ? "" : $" {message}");
        }

        return reference;
    }
}
public class XmlBasedTransformerPipeline : ITransformerPipeline
{
    private readonly Lazy<Task<XDocument>> _document;
    private readonly string _pathToXmlFile;

    public XmlBasedTransformerPipeline(string pathToXmlFile)
    {
        _pathToXmlFile = pathToXmlFile;
        _document = new Lazy<Task<XDocument>>(async () => await XDocument.LoadAsync(File.OpenRead(pathToXmlFile), default, CancellationToken.None));
    }

    async Task ITransformerPipeline.ProcessAsync()
    {
        var doc = await _document.Value;

        var workingDir = Path.GetDirectoryName(_pathToXmlFile).Require();
        var inputEl = doc.XPathSelectElement("pipeline/input").Require();
        var input = new PipelineInputFactory(workingDir)
            .CreateFrom(inputEl.Attribute("type").Require().Value, inputEl.Attribute("name").Require().Value);

        Console.WriteLine(input);
        Stream result = input.Resolve();
        foreach (var step in doc.XPathSelectElements("pipeline/steps/*"))
        {
            var temp = await ApplyStepAsync(step, result);
            await result.DisposeAsync();
            result = temp;
            ////var stepElements = step.Elements();
            ////foreach (var stepElement in stepElements)
            ////{
            ////    Console.WriteLine(stepElement.Name);
            ////}
        }

        var outputEl = doc.XPathSelectElement("pipeline/output").Require();
        IPipelineOutput output = new FileSystemPipelineOutput(Path.Combine(workingDir, outputEl.Attribute("name").Require().Value));
        {
            result.Seek(0, SeekOrigin.Begin);
        }
        await output.WriteAsync(result);
    }

    private static async Task<Stream> ApplyStepAsync(XElement step, Stream input)
    {
        // TODO remove
        await ValueTask.CompletedTask;

        var result = new MemoryStream();
        if (step.Name == "replace-tokens")
        {
            var replacements = step.Elements()
                .Where(e => e.Name == "token")
                .Select(e => (e.Attribute("placeholder").Require().Value, e.FirstNode))
                .ToArray();

            using var reader = new StreamReader(input);
            var writer = new StreamWriter(result);

            while (reader.ReadLine() is string line)
            {
                var resultingLine = line;
                foreach (var (placeholder, tokenNode) in replacements)
                {
                    // TODO tokenNode might be various things, such as e.g. value, environment-variable or something else.
                    var replaceWith = tokenNode!.CreateNavigator().InnerXml;
                    resultingLine = resultingLine.Replace(placeholder, replaceWith);
                }
                writer.WriteLine(resultingLine);
            }
            await writer.FlushAsync();
            return result;
        }
        else
        {
            throw new InvalidOperationException($"Unknown step {step}");
        }
    }
}

public interface IPipelineOutput
{
    Task WriteAsync(Stream data);
}
public record FileSystemPipelineOutput : IPipelineOutput
{
    public FileSystemPipelineOutput(string absolutePath)
    {
        AbsolutePath = Path.GetFullPath(absolutePath);
    }

    public string AbsolutePath { get; }

    async Task IPipelineOutput.WriteAsync(Stream data)
    {
        // truncate or create file
        await File.WriteAllTextAsync(AbsolutePath, "");
        await using var outputStream = File.OpenWrite(AbsolutePath);
        await data.CopyToAsync(outputStream);
        await outputStream.FlushAsync();
    }
}

public class PipelineInputFactory
{
    private readonly string _baseDirectory;

    public PipelineInputFactory(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public IPipelineInput CreateFrom(string type, string name)
    {
        if (type is "filesystem")
        {
            return new FileSystemPipelineInput(Path.Combine(_baseDirectory, name));
        }

        throw new InvalidOperationException("unknown type");
    }
}

public interface IPipelineInput
{
    Stream Resolve();
}

public record FileSystemPipelineInput : IPipelineInput
{
    public FileSystemPipelineInput(string absolutePath)
    {
        AbsolutePath = Path.GetFullPath(absolutePath);
    }

    public string AbsolutePath { get; }

    Stream IPipelineInput.Resolve()
    {
        return File.OpenRead(AbsolutePath);
    }
}
