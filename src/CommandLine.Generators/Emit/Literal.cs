using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CommandLine.Generators.Emit;

/// <summary>
/// Defines methods for emitting literal values in generated code. This class provides utilities to create
/// literal expressions for various data types, ensuring that the generated code is syntactically correct
/// and properly formatted. It can handle literals for primitive types, strings, characters, and other
/// common data types used in C#.
/// </summary>
internal static class Literal
{
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Attempts to format specified value as a literal expression that can be emitted in C# code.
    /// </summary>
    /// <param name="value">The value to get literal for.</param>
    /// <param name="type">Type of the value object.</param>
    /// <param name="literal">[out] String literal representation of the specified value.</param>
    internal static bool TryFormat(object? value, ITypeSymbol type, out string literal)
    {
        if (value is null)
        {
            literal = "null";

            return true;
        }

        // Unwrap Nullable<T> to its underlying type T.
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

        try
        {
            literal = FormatPrimitiveType(value);

            return true;
        }
        catch (NotSupportedException)
        {
            literal = "";

            return false;
        }
    }

    private static string FormatEnumLiteral(INamedTypeSymbol enumType, object value)
    {
        foreach (ISymbol member in enumType.GetMembers())
        {
            if (member is IFieldSymbol field && field.HasConstantValue && Equals(field.ConstantValue, value))
                return $"{enumType.ToDisplayString()}.{field.Name}";
        }

        // If no matching named constant is found (e.g. for flags enum value with multiple bits set), format
        // the value as the underlying numeric type.
        return $"({enumType.ToDisplayString()})" +
            Convert.ToInt64(value, DefaultCulture).ToString(DefaultCulture);
    }

    private static string FormatPrimitiveType(object value)
    {
        return value switch
        {
            string s => SymbolDisplay.FormatLiteral(s, quote: true),
            char c => SymbolDisplay.FormatLiteral(c, quote: true),
            bool b => b ? "true" : "false",

            byte b => b.ToString(DefaultCulture),
            sbyte b => b.ToString(DefaultCulture),
            short s => s.ToString(DefaultCulture),
            ushort s => s.ToString(DefaultCulture),
            int i => i.ToString(DefaultCulture),
            uint i => i.ToString(DefaultCulture) + "u",
            long l => l.ToString(DefaultCulture) + "L",
            ulong l => l.ToString(DefaultCulture) + "UL",

            float f => f.ToString("R", DefaultCulture) + "f",
            double d => d.ToString("R", DefaultCulture) + "d",
            decimal d => d.ToString(DefaultCulture) + "m",

            _ => throw new NotSupportedException(
                $"Literal formatting is not implemented for {value.GetType().FullName}."),
        };
    }
}
