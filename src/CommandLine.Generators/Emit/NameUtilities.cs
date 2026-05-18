using System.Text;

namespace CommandLine.Generators.Emit;

/// <summary>
/// Helper methods for generating CLI option names.
/// </summary>
internal static class NameUtilities
{
    /// <summary>
    /// Converts a camelCase or PascalCase identifier to kebab-case (e.g. "rootPath" -> "root-path").
    /// </summary>
    /// <param name="value">The identifier to convert.</param>
    /// <returns>The kebab-case representation of the identifier.</returns>
    public static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        StringBuilder sb = new(value.Length + 4);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && (char.IsLower(value[i - 1])
                    || (i + 1 < value.Length && char.IsLower(value[i + 1]))))
                {
                    sb.Append('-');
                }

                sb.Append(char.ToLowerInvariant(c));
            }
            else if (c is '_')
                sb.Append('-');
            else
                sb.Append(c);
        }

        return sb.ToString();
    }
}
