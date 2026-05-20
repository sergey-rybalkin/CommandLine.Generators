using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CommandLine.Generators.Models;

/// <summary>
/// Stores metadata from the CommandAttribute applied to a class.
/// </summary>
internal readonly struct CommandHandlerModel : IEquatable<CommandHandlerModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandlerModel"/> struct.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    /// <param name="className">The name of the class that handles the command.</param>
    /// <param name="namespaceName">The namespace of the class.</param>
    /// <param name="hasExecute">True if target type declares execute method, false if not.</param>
    /// <param name="hasAsyncExecute">True if target type declares an async execute method.</param>
    /// <param name="parameters">The command parameters extracted from handler method options.</param>
    /// <param name="location">The source location of the command handler class declaration.</param>
    /// <param name="isPartial">True if the target type is declared as partial, false otherwise.</param>
    /// <param name="hasMultipleConstructors">True if the target type declares multiple constructors.</param>
    public CommandHandlerModel(
        string name,
        string description,
        string className,
        string namespaceName,
        bool hasExecute,
        bool hasAsyncExecute,
        ImmutableArray<CommandParameterModel> parameters,
        Location location,
        bool isPartial,
        bool hasMultipleConstructors)
    {
        Name = name;
        Description = description;
        ClassName = className;
        NamespaceName = namespaceName;
        HasExecuteMethod = hasExecute;
        HasAsyncExecute = hasAsyncExecute;
        Parameters = parameters;
        Location = location;
        IsPartial = isPartial;
        HasMultipleConstructors = hasMultipleConstructors;
    }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the name of the class that handles the command.
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// Gets the namespace of the class.
    /// </summary>
    public string NamespaceName { get; }

    /// <summary>
    /// Gets a value indicating whether target type defines execute method.
    /// </summary>
    public bool HasExecuteMethod { get; }

    /// <summary>
    /// Gets a value indicating whether target type defines an async execute method.
    /// </summary>
    public bool HasAsyncExecute { get; }

    /// <summary>
    /// Gets the command parameters extracted from the handler method.
    /// </summary>
    public ImmutableArray<CommandParameterModel> Parameters { get; }

    /// <summary>
    /// Gets the source location of the command handler class declaration.
    /// </summary>
    public Location Location { get; }

    /// <summary>
    /// Gets a value indicating whether the target type is declared as partial.
    /// </summary>
    public bool IsPartial { get; }

    /// <summary>
    /// Gets a value indicating whether the target type declares multiple constructors.
    /// </summary>
    public bool HasMultipleConstructors { get; }

    /// <inheritdoc/>
    public bool Equals(CommandHandlerModel other)
    {
        if (!string.Equals(Name, other.Name, StringComparison.Ordinal)
            || !string.Equals(Description, other.Description, StringComparison.Ordinal)
            || !string.Equals(ClassName, other.ClassName, StringComparison.Ordinal)
            || !string.Equals(NamespaceName, other.NamespaceName, StringComparison.Ordinal)
            || HasExecuteMethod != other.HasExecuteMethod
            || HasAsyncExecute != other.HasAsyncExecute
            || IsPartial != other.IsPartial
            || HasMultipleConstructors != other.HasMultipleConstructors
            || Parameters.Length != other.Parameters.Length)
        {
            return false;
        }

        for (int i = 0; i < Parameters.Length; i++)
        {
            if (!Parameters[i].Equals(other.Parameters[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is CommandHandlerModel other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(Name ?? "");
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(Description ?? "");
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(ClassName ?? "");
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(NamespaceName ?? "");
            hash = (hash * 31) + HasExecuteMethod.GetHashCode();
            hash = (hash * 31) + HasAsyncExecute.GetHashCode();
            hash = (hash * 31) + IsPartial.GetHashCode();
            hash = (hash * 31) + HasMultipleConstructors.GetHashCode();

            foreach (CommandParameterModel parameter in Parameters)
                hash = (hash * 31) + parameter.GetHashCode();

            return hash;
        }
    }
}
