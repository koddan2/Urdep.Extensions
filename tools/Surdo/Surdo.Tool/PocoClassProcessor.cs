using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Surdo.Tool;

public class PocoClassProcessor : CSharpSyntaxWalker
{
    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // Check if the class is a POCO class (e.g., has properties but no methods)
        var properties = node.Members.OfType<PropertyDeclarationSyntax>().ToList();
        var methods = node.Members.OfType<MethodDeclarationSyntax>().ToList();

        if (properties.Any() && !methods.Any())
        {
            // Generate a new class based on the POCO class
            var newClass = GenerateNewClass(node, properties);
            // Output or save the new class
            _ = newClass;
        }

        base.VisitClassDeclaration(node);
    }

    private ClassDeclarationSyntax GenerateNewClass(
        ClassDeclarationSyntax pocoClass,
        List<PropertyDeclarationSyntax> properties
    )
    {
        // Create a new class declaration
        var newClass = SyntaxFactory
            .ClassDeclaration(pocoClass.Identifier.Text + "Runtime")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        // Add a method that performs runtime tasks
        var method = SyntaxFactory
            .MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "PerformRuntimeTasks"
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(
                SyntaxFactory.Block(
                    properties.Select(p =>
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory
                                .InvocationExpression(
                                    SyntaxFactory.IdentifierName("Console.WriteLine")
                                )
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName(p.Identifier)
                                            )
                                        )
                                    )
                                )
                        )
                    )
                )
            );

        newClass = newClass.AddMembers(method);

        return newClass;
    }
}
