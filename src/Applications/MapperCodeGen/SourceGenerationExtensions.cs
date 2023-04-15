using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapperCodeGen;

internal static class SourceGenerationExtensions
{
    internal static string BuildDtoProperty(
        this PropertyDeclarationSyntax pds, Compilation compilation)
    {
        // get the symbol for this property from the semantic model
        var symbol = compilation
                    .GetSemanticModel(pds.SyntaxTree)
                    .GetDeclaredSymbol(pds);

        if (symbol is IPropertySymbol property)
        {
            // use the same type and name for the DTO properties as on the entity
            return $"public {property.Type.Name()} {property.Name} {{ get; set; }}";
        }

        return "";
    }

    // instead of returning "System.Collections.Generic.IList<>", just condense it to "IList<>"
    // the namespace is already added in the usings block
    internal static string Name(this ITypeSymbol typeSymbol) =>
        typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
}
