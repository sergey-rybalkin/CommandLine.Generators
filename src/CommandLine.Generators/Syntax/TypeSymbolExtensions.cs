using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CommandLine.Generators.Syntax;

internal static class TypeSymbolExtensions
{
    internal static readonly SymbolDisplayFormat HumanReadableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    internal static IMethodSymbol? LookupMethodInClassHierarchy(
        this INamedTypeSymbol containingType,
        Compilation compilation,
        string methodName,
        Func<ImmutableArray<IParameterSymbol>, bool> validateSignature,
        bool isAsync = false,
        SpecialType returnValue = SpecialType.System_Int32)
    {
        for (INamedTypeSymbol? current = containingType; current is not null; current = current.BaseType)
        {
            foreach (IMethodSymbol method in current.GetMembers(methodName).OfType<IMethodSymbol>())
            {
                if (method.IsStatic || method.IsAbstract || method.TypeParameters.Length > 0)
                    continue;

                if (method.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal)
                    continue;

                ITypeSymbol returnType;
                ImmutableArray<IParameterSymbol> signature = method.Parameters;
                if (isAsync)
                    returnType = GetAsyncReturnType(returnValue, compilation);
                else
                    returnType = compilation.GetSpecialType(returnValue);

                if (!method.ReturnType.Equals(returnType, SymbolEqualityComparer.Default) ||
                    !validateSignature(signature))
                {
                    continue;
                }

                return method;
            }
        }

        return null;
    }

    private static INamedTypeSymbol GetAsyncReturnType(SpecialType baseType, Compilation compilation)
    {
        INamedTypeSymbol taskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")
            ?? throw new InvalidOperationException("Could not find Task<T>.");
        ITypeSymbol baseTypeValue = compilation.GetSpecialType(baseType);

        return taskOfT.Construct(baseTypeValue);
    }
}
