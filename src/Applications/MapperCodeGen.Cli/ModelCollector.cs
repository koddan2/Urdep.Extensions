using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace MapperCodeGen.Cli;

internal class ModelCollector : CSharpSyntaxWalker
{
    public Dictionary<string, List<Dictionary<string, object>>> Models { get; } =
        new Dictionary<string, List<Dictionary<string, object>>>();

    public ILogger? Log { get; set; }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (node.Parent is ClassDeclarationSyntax classnode)
        {
            if (!Models.ContainsKey(classnode.Identifier.ValueText))
            {
                var list = new List<Dictionary<string, object>>();
                Models.Add(classnode.Identifier.ValueText, list);
                var classInfo = new Dictionary<string, object>
                {
                    ["Identifier"] = classnode.Identifier.ValueText,
                    ["BaseList"] = classnode.BaseList?.ToString() ?? "",
                    ["Modifiers"] = classnode.Modifiers.ToString(),
                    ["Attributes"] = classnode.AttributeLists
                        .SelectMany(
                            x =>
                                x.Attributes.Select(
                                    a => new[] { a.Name.ToString(), a.ArgumentList?.ToString() }
                                )
                        )
                        .ToList()
                };
                list.Add(classInfo);
            }

            var info = new Dictionary<string, object>
            {
                ["Identifier"] = node.Identifier.ValueText,
                ["Type"] = node.Type.ToString(),
                ["TypeEx"] = node.ChildNodes().First() is GenericNameSyntax gen
                    ? gen.Identifier.ValueText
                    : "",
                ["Modifiers"] = node.Modifiers.ToString(),
                ["Attributes"] = node.AttributeLists
                    .SelectMany(
                        x =>
                            x.Attributes.Select(
                                a => new[] { a.Name.ToString(), a.ArgumentList?.ToString() }
                            )
                    )
                    .ToList()
            };
            Models[classnode.Identifier.ValueText].Add(info);
        }
        else
        {
            var ident = "";
            if (node.Parent is BaseTypeDeclarationSyntax baseType)
            {
                ident = baseType.Identifier.ValueText;
            }
            Log?.LogInformation(
                "Skipping {syntax} {identifier}",
                node.Parent!.GetType().Name,
                ident
            );
            // interface or other such thing
        }
    }
}
