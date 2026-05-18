namespace CommandLine.Generators.Models;

/// <summary>
/// Stores metadata from the OptionAttribute applied to a parameter.
/// </summary>
internal readonly struct CommandParameterModel : IEquatable<CommandParameterModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandParameterModel"/> struct.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterTypeName">The simplified type name of the parameter.</param>
    /// <param name="description">The description of the option.</param>
    /// <param name="valueHint">The value hint for the option.</param>
    /// <param name="alias">The single character alias for the option.</param>
    /// <param name="isNullable">True if the parameter type is nullable.</param>
    /// <param name="hasDefaultValue">True if the parameter has an explicit default value.</param>
    /// <param name="defaultValueLiteral">
    /// The C# literal representation of the default value, when supported.
    /// </param>
    public CommandParameterModel(
        string parameterName,
        string parameterTypeName,
        string description,
        string? valueHint = null,
        char? alias = null,
        bool isNullable = false,
        bool hasDefaultValue = false,
        string? defaultValueLiteral = null)
    {
        ParameterName = parameterName;
        ParameterTypeName = parameterTypeName;
        Description = description;
        ValueHint = valueHint;
        Alias = alias;
        IsNullable = isNullable;
        HasDefaultValue = hasDefaultValue;
        DefaultValueLiteral = defaultValueLiteral;
    }

    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Gets the simplified type name of the parameter.
    /// </summary>
    public string ParameterTypeName { get; }

    /// <summary>
    /// Gets the description of the option.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the value hint for the option.
    /// </summary>
    public string? ValueHint { get; }

    /// <summary>
    /// Gets the single character alias for the option.
    /// </summary>
    public char? Alias { get; }

    /// <summary>
    /// Gets a value indicating whether the parameter type is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets a value indicating whether the parameter has an explicit default value.
    /// </summary>
    public bool HasDefaultValue { get; }

    /// <summary>
    /// Gets the C# literal representation of the default value, when available.
    /// </summary>
    public string? DefaultValueLiteral { get; }

    /// <inheritdoc/>
    public bool Equals(CommandParameterModel other)
    {
        return string.Equals(ParameterName, other.ParameterName, StringComparison.Ordinal)
            && string.Equals(ParameterTypeName, other.ParameterTypeName, StringComparison.Ordinal)
            && string.Equals(Description, other.Description, StringComparison.Ordinal)
            && string.Equals(ValueHint, other.ValueHint, StringComparison.Ordinal)
            && Alias == other.Alias
            && IsNullable == other.IsNullable
            && HasDefaultValue == other.HasDefaultValue
            && string.Equals(DefaultValueLiteral, other.DefaultValueLiteral, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is CommandParameterModel other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(ParameterName ?? "");
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(ParameterTypeName ?? "");
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(Description ?? "");
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(ValueHint ?? "");
            hash = (hash * 31) + (Alias.HasValue ? Alias.Value.GetHashCode() : 0);
            hash = (hash * 31) + IsNullable.GetHashCode();
            hash = (hash * 31) + HasDefaultValue.GetHashCode();
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(DefaultValueLiteral ?? "");

            return hash;
        }
    }
}
