using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapperCodeGen;

internal static class SourceAnalysisExtensions
{
    internal static bool IsDecoratedWithAttribute(
        this TypeDeclarationSyntax cdecl,
        string attributeName
    )
    {
        var attrs = cdecl.AttributeLists.SelectMany(x => x.Attributes);
        return HasAttribute(attributeName, attrs);
    }

    private static bool HasAttribute(string attributeName, IEnumerable<AttributeSyntax> attrs)
    {
        foreach (var attr in attrs)
        {
            if (attr.Name.ToString() == attributeName || $"{attr.Name}Attribute" == attributeName)
            {
                return true;
            }
        }

        return false;
    }
}
