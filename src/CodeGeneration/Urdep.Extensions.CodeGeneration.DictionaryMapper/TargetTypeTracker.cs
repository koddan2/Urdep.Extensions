using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Urdep.Extensions.CodeGeneration.DictionaryMapper;

public class TargetTypeTracker : ISyntaxContextReceiver
{
    public IImmutableList<TypeDeclarationSyntax> TypesToGenerateFor =
        ImmutableList.Create<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (
            context.Node is TypeDeclarationSyntax cdecl
            && cdecl.IsDecoratedWithAttribute(
                nameof(GenerateDictionaryMappingExtensionMethodsAttribute)
            )
            && context.Node is TypeDeclarationSyntax typeDeclSyn
        )
        {
            TypesToGenerateFor = TypesToGenerateFor.Add(typeDeclSyn);
        }
    }
}
