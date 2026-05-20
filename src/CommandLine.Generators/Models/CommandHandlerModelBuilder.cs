using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommandLine.Generators.Models;

/// <summary>
/// Provides functionality to build <see cref="CommandHandlerModel"/> instances from syntax context.
/// </summary>
internal static class CommandHandlerModelBuilder
{
    internal static CommandHandlerModel FromSyntaxContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken ct)
    {
        INamedTypeSymbol containingType = (INamedTypeSymbol)context.TargetSymbol;

        // Only top-level classes in a named namespace are supported.
        if (containingType.ContainingType is not null
            || containingType.ContainingNamespace.IsGlobalNamespace)
        {
            return default;
        }

        // Look for Execute method that can be set as an action handler for the command.
        bool hasSyncExecute = containingType.GetMembers(Constants.ExecuteMethodName)
            .OfType<IMethodSymbol>()
            .Any(m =>
                !m.IsStatic &&
                m.Parameters.Length == 0 &&
                m.ReturnType.SpecialType == SpecialType.System_Int32);

        bool hasAsyncExecute = containingType.GetMembers(Constants.ExecuteAsyncMethodName)
            .OfType<IMethodSymbol>()
            .Any(IsAsyncExecuteSignature);

        Location location = containingType.Locations[0];

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
            location,
            isPartial,
            hasMultipleConstructors);
    }

    private static bool IsAsyncExecuteSignature(IMethodSymbol method)
    {
        // Should be async Task<int> ExecuteAsync(CancellationToken ct)
        if (method.IsStatic || method.Parameters.Length is not 1)
            return false;

        if (method.ReturnType is not INamedTypeSymbol returnType
            || returnType.TypeArguments.Length is not 1
            || returnType.TypeArguments[0].SpecialType is not SpecialType.System_Int32
            || returnType.ConstructedFrom.ToDisplayString() is not "System.Threading.Tasks.Task<TResult>")
        {
            return false;
        }

        IParameterSymbol parameter = method.Parameters[0];

        return parameter.Type.ToDisplayString() is "System.Threading.CancellationToken";
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

            string paramTypeName = parameter.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                    .WithMiscellaneousOptions(
                        SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
                        | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
                        | SymbolDisplayMiscellaneousOptions.UseSpecialTypes));

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

        if (TryFormatLiteral(parameter.ExplicitDefaultValue, parameter.Type, out string literal))
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

    private static bool TryFormatLiteral(object? value, ITypeSymbol type, out string literal)
    {
        if (value is null)
        {
            literal = "null";

            return true;
        }

        // Unwrap Nullable<T> to its underlying type for enum handling.
        ITypeSymbol effectiveType = type;
        if (type is INamedTypeSymbol named
            && named.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T
            && named.TypeArguments.Length is 1)
        {
            effectiveType = named.TypeArguments[0];
        }

        if (effectiveType.TypeKind is TypeKind.Enum)
        {
            literal = FormatEnumLiteral((INamedTypeSymbol)effectiveType, value);

            return true;
        }

        return TryFormatPrimitive(value, out literal);
    }

    private static string FormatEnumLiteral(INamedTypeSymbol enumType, object value)
    {
        foreach (ISymbol member in enumType.GetMembers())
        {
            if (member is IFieldSymbol field && field.HasConstantValue
                && Equals(field.ConstantValue, value))
            {
                return $"{enumType.ToDisplayString()}.{field.Name}";
            }
        }

        return $"({enumType.ToDisplayString()})"
            + Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
    }

    private static bool TryFormatPrimitive(object value, out string literal)
    {
        switch (value)
        {
            case string s:
                literal = SymbolDisplay.FormatLiteral(s, quote: true);
                return true;
            case char c:
                literal = SymbolDisplay.FormatLiteral(c, quote: true);
                return true;
            case bool b:
                literal = b ? "true" : "false";
                return true;
            default:
                return TryFormatNumeric(value, out literal);
        }
    }

    private static bool TryFormatNumeric(object value, out string literal)
    {
        switch (value)
        {
            case byte by:
                literal = by.ToString(CultureInfo.InvariantCulture);
                return true;
            case sbyte sb:
                literal = sb.ToString(CultureInfo.InvariantCulture);
                return true;
            case short sh:
                literal = sh.ToString(CultureInfo.InvariantCulture);
                return true;
            case ushort ush:
                literal = ush.ToString(CultureInfo.InvariantCulture);
                return true;
            case int i:
                literal = i.ToString(CultureInfo.InvariantCulture);
                return true;
            case uint ui:
                literal = ui.ToString(CultureInfo.InvariantCulture) + "U";
                return true;
            case long l:
                literal = l.ToString(CultureInfo.InvariantCulture) + "L";
                return true;
            case ulong ul:
                literal = ul.ToString(CultureInfo.InvariantCulture) + "UL";
                return true;
            case float f:
                literal = f.ToString("R", CultureInfo.InvariantCulture) + "F";
                return true;
            case double d:
                literal = d.ToString("R", CultureInfo.InvariantCulture) + "D";
                return true;
            case decimal m:
                literal = m.ToString(CultureInfo.InvariantCulture) + "M";
                return true;
            default:
                literal = "";
                return false;
        }
    }
}
