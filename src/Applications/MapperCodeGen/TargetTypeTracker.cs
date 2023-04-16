using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace MapperCodeGen;

public class TargetTypeTracker : ISyntaxContextReceiver
{
    public IImmutableList<TypeDeclarationSyntax> TypesNeedingDtoGening =
        ImmutableList.Create<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is TypeDeclarationSyntax cdecl)
        {
            if (
                cdecl.IsDecoratedWithAttribute(nameof(GenerateMappedDtoAttribute))
                && context.Node is TypeDeclarationSyntax typeDeclSyn
            )
            {
                TypesNeedingDtoGening = TypesNeedingDtoGening.Add(typeDeclSyn);
            }
        }
    }
}
