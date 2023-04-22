﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Urdep.Extensions.CodeGeneration.DictionaryMapper;

[Generator]
public class DictionaryMappingGenerator : ISourceGenerator
{
    private const string _INDENT = "    ";

    private static readonly DiagnosticDescriptor _InvalidTypeWarning =
        new(
            id: "DMG0001",
            title: "Couldn't determine actual type",
            messageFormat: "Couldn't determine actual type '{0}'",
            category: nameof(DictionaryMappingGenerator),
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new TargetTypeTracker());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is TargetTypeTracker targetTypeTracker)
        {
            ExecuteInner(context, targetTypeTracker);
        }
    }

    private static void AppendHeader(StringBuilder codeBuilder)
    {
        var assembly = typeof(DictionaryMappingGenerator).Assembly;
        codeBuilder
            .AppendFormat(
                @"// <auto-generated>
//     This code is machine-generated.
//     Assembly: {0}
// </auto-generated>
",
                assembly.FullName
            )
            .AppendLine();
        codeBuilder.AppendLine("#nullable enable");
        codeBuilder.AppendLine(
            "#pragma warning disable IDE0017 // Object initialization can be simplified"
        );
        codeBuilder.AppendLine(
            "#pragma warning disable IDE0028 // Collection initialization can be simplified"
        );
    }

    private static void AppendTrailer(StringBuilder codeBuilder)
    {
        codeBuilder.AppendLine(
            "#pragma warning restore IDE0017 // Object initialization can be simplified"
        );
        codeBuilder.AppendLine(
            "#pragma warning restore IDE0028 // Collection initialization can be simplified"
        );
    }

    public static void ExecuteInner(
        GeneratorExecutionContext context,
        TargetTypeTracker targetTypeTracker
    )
    {
        var codeBuilder = new StringBuilder();

        foreach (var typeDeclSyn in targetTypeTracker.TypesToGenerateFor)
        {
            AppendHeader(codeBuilder);
            // Use the semantic model to get the symbol for this type
            var typeNodeSymbol = context.Compilation
                .GetSemanticModel(typeDeclSyn.SyntaxTree)
                .GetDeclaredSymbol(typeDeclSyn);
            if (typeNodeSymbol is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(_InvalidTypeWarning, null, typeDeclSyn));
                continue;
            }

            // get the namespace of the entity class
            var entityClassNamespace =
                typeNodeSymbol.ContainingNamespace?.ToDisplayString()
                ?? "__GeneratedDefaultNamespace__";

            // suffix the base type's name to make the generated class' name
            var generatedExtensionMethodClassName =
                $"{typeNodeSymbol.Name}DictionaryMappingExtensions";

            // Add usings
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using System.Linq;");

            // Add target namespace
            codeBuilder
                .Append("namespace ")
                .Append(entityClassNamespace)
                .AppendLine(".DictionaryMapping");
            codeBuilder.AppendLine("{");
#if DEBUG_GENERATOR
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            // Start class
            codeBuilder
                .Append(_INDENT)
                .Append("public static class ")
                .AppendLine(generatedExtensionMethodClassName);
            codeBuilder.Append(_INDENT).AppendLine("{");

            AppendToDictionaryMethod(codeBuilder, typeNodeSymbol, typeDeclSyn);
            codeBuilder.AppendLine();
            AppendIntoDictionaryMethod(codeBuilder, typeNodeSymbol, typeDeclSyn);
            codeBuilder.AppendLine();
            AppendFromDictionaryMethod(codeBuilder, typeNodeSymbol, typeDeclSyn);

            codeBuilder.Append(_INDENT).AppendLine("}");
            codeBuilder.AppendLine("}");

            AppendTrailer(codeBuilder);
            var code = codeBuilder.ToString();
            // add the code for this DTO class to the context so it can be added to the build
            context.AddSource(
                generatedExtensionMethodClassName,
                SourceText.From(code, Encoding.UTF8)
            );
            codeBuilder.Clear();
        }
    }

    private static void AppendFromDictionaryMethod(
        StringBuilder codeBuilder,
        ISymbol typeNodeSymbol,
        TypeDeclarationSyntax typeDeclSyn
    )
    {
        codeBuilder
            .Append(_INDENT)
            .Append(_INDENT)
            .Append("public static ")
            .Append(typeNodeSymbol.Name)
            .Append(' ')
            .Append("FromDictionaryTo")
            .Append(typeNodeSymbol.Name)
            .AppendLine("(this IDictionary<string, object?> dictionary)");

        codeBuilder.Append(_INDENT).Append(_INDENT).AppendLine("{");
        codeBuilder
            .Append(_INDENT)
            .Append(_INDENT)
            .Append(_INDENT)
            .Append("var result = new ")
            .Append(typeNodeSymbol.Name)
            .AppendLine("(");
        if (
            typeDeclSyn is RecordDeclarationSyntax recordDeclSyn
            && recordDeclSyn.ParameterList is ParameterListSyntax paramListSyn
        )
        {
            var arr = paramListSyn.Parameters.ToArray();
            for (int i = 0; i < arr.Length; ++i)
            {
                var parameter = arr[i];
                codeBuilder.Append(_INDENT).Append(_INDENT).Append(_INDENT).Append(_INDENT);
                AppendCast(codeBuilder, parameter.Type!.ToFullString(), parameter.Identifier);
                if (i < arr.Length - 1)
                {
                    codeBuilder.Append(",");
                }
                codeBuilder.AppendLine();
            }
        }

        codeBuilder.Append(_INDENT).Append(_INDENT).Append(_INDENT).AppendLine(");");
        // get all the properties defined in this class
        var allProperties = typeDeclSyn.Members.OfType<PropertyDeclarationSyntax>();

        foreach (var property in allProperties.ToArray())
        {
            codeBuilder
                .Append(_INDENT)
                .Append(_INDENT)
                .Append(_INDENT)
                .Append("result.")
                .Append(property.Identifier)
                .AppendLine(" =");
            //.Append(" = ");
            //// cast
            //.Append('(').Append(property.Type!.ToFullString().Trim())
            //.Append(")dictionary[\"")
            //.Append(property.Identifier)
            //.AppendLine("\"];");
            codeBuilder.Append(_INDENT).Append(_INDENT).Append(_INDENT).Append(_INDENT);
            AppendCast(codeBuilder, property.Type!.ToFullString(), property.Identifier);
            codeBuilder.AppendLine(";");
        }

        codeBuilder.Append(_INDENT).Append(_INDENT).Append(_INDENT).AppendLine("return result;");

        // Add closing braces
        codeBuilder.Append(_INDENT).Append(_INDENT).AppendLine("}");
    }

    private static void AppendToDictionaryMethod(
        StringBuilder codeBuilder,
        ISymbol typeNodeSymbol,
        TypeDeclarationSyntax typeDeclSyn
    )
    {
        codeBuilder
            .Append(_INDENT)
            .Append(_INDENT)
            .Append("public static IDictionary<string, object?> ToDictionary(this ")
            .Append(typeNodeSymbol.Name)
            .AppendLine(" instance)");
        codeBuilder.Append(_INDENT).Append(_INDENT).AppendLine("{");
        codeBuilder
            .Append(_INDENT)
            .Append(_INDENT)
            .Append(_INDENT)
            .AppendLine("var result = new Dictionary<string, object?>();");
        if (
            typeDeclSyn is RecordDeclarationSyntax recordDeclSyn
            && recordDeclSyn.ParameterList is ParameterListSyntax paramListSyn
        )
        {
            foreach (var parameter in paramListSyn.Parameters)
            {
                codeBuilder
                    .Append(_INDENT)
                    .Append(_INDENT)
                    .Append(_INDENT)
                    .Append("result[\"")
                    .Append(parameter.Identifier)
                    .Append("\"] = instance.")
                    .Append(parameter.Identifier)
                    .AppendLine(";");
            }
        }
        // get all the properties defined in this class
        var allProperties = typeDeclSyn.Members.OfType<PropertyDeclarationSyntax>();

        // for each property in the domain entity, create a corresponding property
        // in the DTO with the same type
        foreach (var property in allProperties.ToArray())
        {
            codeBuilder
                .Append(_INDENT)
                .Append(_INDENT)
                .Append(_INDENT)
                .Append("result[\"")
                .Append(property.Identifier)
                .Append("\"] = instance.")
                .Append(property.Identifier)
                .AppendLine(";");
        }

        codeBuilder.Append(_INDENT).Append(_INDENT).Append(_INDENT).AppendLine("return result;");

        // Add closing braces
        codeBuilder.Append(_INDENT).Append(_INDENT).AppendLine("}");
    }

    private static void AppendIntoDictionaryMethod(
        StringBuilder codeBuilder,
        ISymbol typeNodeSymbol,
        TypeDeclarationSyntax typeDeclSyn
    )
    {
        codeBuilder
            .Append(_INDENT)
            .Append(_INDENT)
            .Append("public static void IntoDictionary(this ")
            .Append(typeNodeSymbol.Name)
            .AppendLine(" instance, IDictionary<string, object?> dictionary)");
        codeBuilder.Append(_INDENT).Append(_INDENT).AppendLine("{");
        if (
            typeDeclSyn is RecordDeclarationSyntax recordDeclSyn
            && recordDeclSyn.ParameterList is ParameterListSyntax paramListSyn
        )
        {
            foreach (var parameter in paramListSyn.Parameters)
            {
                codeBuilder
                    .Append(_INDENT)
                    .Append(_INDENT)
                    .Append(_INDENT)
                    .Append("dictionary[\"")
                    .Append(parameter.Identifier)
                    .Append("\"] = instance.")
                    .Append(parameter.Identifier)
                    .AppendLine(";");
            }
        }
        // get all the properties defined in this class
        var allProperties = typeDeclSyn.Members.OfType<PropertyDeclarationSyntax>();

        // for each property in the domain entity, create a corresponding property
        // in the DTO with the same type
        foreach (var property in allProperties.ToArray())
        {
            codeBuilder
                .Append(_INDENT)
                .Append(_INDENT)
                .Append(_INDENT)
                .Append("dictionary[\"")
                .Append(property.Identifier)
                .Append("\"] = instance.")
                .Append(property.Identifier)
                .AppendLine(";");
        }

        // Add closing braces
        codeBuilder.Append(_INDENT).Append(_INDENT).AppendLine("}");
    }

    private static void AppendCast(
        StringBuilder codeBuilder,
        string typeFullString,
        SyntaxToken identifier
    )
    {
        var trimmedTypeFullString = typeFullString.Trim();
        var isNullable = trimmedTypeFullString.EndsWith("?");
        codeBuilder
            // cast
            .Append('(')
            .Append(trimmedTypeFullString);
        if (isNullable)
        {
            //codeBuilder.Append(")dictionary[\"").Append(identifier).Append("\"]");
            codeBuilder
                .Append(")(dictionary.TryGetValue(\"")
                .Append(identifier)
                .AppendFormat("\", out var {0}) && {0} is not null ? {0} : null)", identifier);
        }
        else
        {
            codeBuilder.AppendFormat(
                ")(dictionary.TryGetValue(\"{0}\", out var {0}) ? {0} ?? throw new ArgumentException(\"The value for the key '{0}' was null\", nameof(dictionary)) : throw new KeyNotFoundException(\"The key '{0}' was not found in the dictionary.\"))",
                identifier
            );
        }
    }
}
