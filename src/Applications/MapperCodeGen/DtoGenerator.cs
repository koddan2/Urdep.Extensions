using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace MapperCodeGen;

[Generator]
public class DtoGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is TargetTypeTracker targetTypeTracker)
        {
            ExecuteInner(context, targetTypeTracker);
        }
    }

    public static void ExecuteInner(
        GeneratorExecutionContext context,
        TargetTypeTracker targetTypeTracker
    )
    {
        var codeBuilder = new StringBuilder();

        foreach (var typeNode in targetTypeTracker.TypesNeedingDtoGening)
        {
            // Use the semantic model to get the symbol for this type
            var typeNodeSymbol = context.Compilation
                .GetSemanticModel(typeNode.SyntaxTree)
                .GetDeclaredSymbol(typeNode);
            if (typeNodeSymbol is null)
            {
                continue;
            }

            // get the namespace of the entity class
            var entityClassNamespace =
                typeNodeSymbol.ContainingNamespace?.ToDisplayString() ?? "NoNamespace";

            // give each DTO a name, just suffix the entity class name with "Dto"
            var generatedDtoClassName = $"{typeNodeSymbol.Name}Dto";

            // Add usings
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using System.Linq;");

            // Add target namespace
            codeBuilder.Append("namespace ").Append(entityClassNamespace).AppendLine(".Dtos");
            codeBuilder.AppendLine("{");

            // Start class
            codeBuilder.Append("\tpublic class ").AppendLine(generatedDtoClassName);
            codeBuilder.AppendLine("\t{");

            // get all the properties defined in this class
            var allProperties = typeNode.Members.OfType<PropertyDeclarationSyntax>();

            // for each property in the domain entity, create a corresponding property
            // in the DTO with the same type
            foreach (var property in allProperties)
            {
                codeBuilder
                    .Append("\t\t")
                    .AppendLine(property.BuildDtoProperty(context.Compilation));
            }

            // Add closing braces
            codeBuilder.AppendLine("\t}");
            codeBuilder.AppendLine("}");

            // add the code for this DTO class to the context so it can be added to the build
            context.AddSource(
                generatedDtoClassName,
                SourceText.From(codeBuilder.ToString(), Encoding.UTF8)
            );
            codeBuilder.Clear();
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new TargetTypeTracker());
    }
}
