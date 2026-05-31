using System.Collections.Immutable;
using CommandLine.Generators.Emit;
using CommandLine.Generators.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommandLine.Generators.Models;

/// <summary>
/// Provides functionality to build <see cref="CommandHandlerModel"/> instances from syntax context.
/// </summary>
/// <remarks>
/// Do not validate syntax or throw any exceptions here. If the syntax is invalid, simply return a model
/// with default values or missing information. The generator should be able to handle such cases
/// gracefully and report diagnostics as needed without relying on exceptions from this builder.
/// </remarks>
internal static class CommandHandlerModelBuilder
{
    internal static CommandHandlerModel FromSyntaxContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken ct)
    {
        INamedTypeSymbol containingType = (INamedTypeSymbol)context.TargetSymbol;

        ResolveExecuteMethods(
            context.SemanticModel.Compilation,
            containingType,
            out bool hasSyncExecute,
            out bool hasAsyncExecute);

        AttributeData commandAttr = context.Attributes[0];
        string name = (string?)commandAttr.ConstructorArguments[0].Value ?? "";
        string description = (string?)commandAttr.ConstructorArguments[1].Value ?? "";
        string ns = containingType.ContainingNamespace.ToDisplayString();

        bool hasMultipleConstructors = containingType.InstanceConstructors.Length > 1;
        ImmutableArray<CommandParameterModel> parameters = ExtractParameters(containingType, ct);

        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
        bool isPartial = classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);

        return new CommandHandlerModel(
            name,
            description,
            containingType.Name,
            ns,
            hasSyncExecute || hasAsyncExecute,
            !hasSyncExecute && hasAsyncExecute,
            parameters,
            containingType.Locations[0],
            isPartial,
            hasMultipleConstructors,
            containingType.ContainingType is not null);
    }

    private static void ResolveExecuteMethods(
        Compilation compilation,
        INamedTypeSymbol containingType,
        out bool hasSyncExecute,
        out bool hasAsyncExecute)
    {
        IMethodSymbol? candidateMethod = containingType.LookupMethodInClassHierarchy(
            compilation,
            Constants.ExecuteAsyncMethodName,
            p => p.Length is 1 && p[0].Type.ToDisplayString() is "System.Threading.CancellationToken",
            true);
        hasAsyncExecute = candidateMethod is not null;

        candidateMethod = containingType.LookupMethodInClassHierarchy(
            compilation, Constants.ExecuteMethodName, p => p.Length is 0);
        hasSyncExecute = candidateMethod is not null;
    }

    private static ImmutableArray<CommandParameterModel> ExtractParameters(
        INamedTypeSymbol type, CancellationToken ct)
    {
        ImmutableArray<IMethodSymbol> constructors = type.InstanceConstructors;
        if (constructors.Length is not 1)
            return [];

        ImmutableArray<CommandParameterModel>.Builder builder =
            ImmutableArray.CreateBuilder<CommandParameterModel>(constructors[0].Parameters.Length);

        IMethodSymbol constructor = constructors[0];

        foreach (IParameterSymbol parameter in constructor.Parameters)
        {
            ct.ThrowIfCancellationRequested();

            if (TryCreateParameterModel(parameter, out CommandParameterModel parameterModel))
                builder.Add(parameterModel);
        }

        return builder.ToImmutable();
    }

    private static bool TryCreateParameterModel(
        IParameterSymbol parameter,
        out CommandParameterModel parameterModel)
    {
        foreach (AttributeData attr in parameter.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() is not Constants.MetadataOptionAttributeName)
                continue;

            ReadOptionAttribute(attr, out string description, out string? valueHint, out char? alias);

            string paramTypeName = parameter.Type.ToDisplayString(TypeSymbolExtensions.HumanReadableFormat);
            bool isNullable = IsNullable(parameter);
            ResolveDefault(parameter, out bool hasDefault, out string? defaultLiteral);

            parameterModel = new(
                parameter.Name,
                paramTypeName,
                description,
                valueHint,
                alias,
                isNullable,
                hasDefault,
                defaultLiteral);

            return true;
        }

        parameterModel = default;

        return false;
    }

    private static void ReadOptionAttribute(
        AttributeData attr,
        out string description,
        out string? valueHint,
        out char? alias)
    {
        description = attr.ConstructorArguments.Length > 0
            ? (string?)attr.ConstructorArguments[0].Value ?? ""
            : "";

        valueHint = attr.ConstructorArguments.Length > 1
            ? (string?)attr.ConstructorArguments[1].Value
            : null;

        alias = attr.ConstructorArguments.Length > 2
            && attr.ConstructorArguments[2].Value is char aliasChar
            && aliasChar is not '\0'
                ? aliasChar
                : null;
    }

    private static void ResolveDefault(
        IParameterSymbol parameter,
        out bool hasDefault,
        out string? defaultLiteral)
    {
        defaultLiteral = null;
        hasDefault = parameter.HasExplicitDefaultValue;
        if (!hasDefault)
            return;

        if (Literal.TryFormat(parameter.ExplicitDefaultValue, parameter.Type, out string literal))
            defaultLiteral = literal;
        else
            hasDefault = false;
    }

    private static bool IsNullable(IParameterSymbol parameter)
    {
        ITypeSymbol type = parameter.Type;
        if (type.IsValueType)
            return type.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        return parameter.NullableAnnotation is NullableAnnotation.Annotated;
    }
}
