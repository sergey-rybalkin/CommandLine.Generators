namespace CommandLine.Generators;

internal static class Constants
{
    internal const string MetadataAttributeName = "CommandLine.Generators.CommandAttribute";

    internal const string MetadataOptionAttributeName = "CommandLine.Generators.OptionAttribute";

    internal const string AttributesDefinitionFileName = "CommandLine.Attributes.g.cs";

    internal const string ExecuteMethodName = "Execute";

    internal const string ExecuteAsyncMethodName = "ExecuteAsync";

    internal const string GetCommandDefinitionMethodName = "GetCommandDefinition";

    internal const string FromParseResultMethodName = "FromParseResult";

    internal const string OnCommandDefinedMethodName = "OnCommandDefined";

    internal const string OnCommandCreatedMethodName = "OnCommandCreated";

    internal const string RootExtensionsFileName = "RootCommandExtensions.g.cs";

    internal const string GeneratedNamespace = "CommandLine.Generators";

    internal const string AttributesDefinition = """
#nullable enable

using System;
using Microsoft.CodeAnalysis;

namespace CommandLine.Generators;

/// <summary>
/// Marks classes that handle CLI commands.
/// </summary>
/// <param name="name">Name of the command.</param>
/// <param name="description">Command description that will be shown in the help message.</param>
[Embedded]
[AttributeUsage(AttributeTargets.Class)]
internal sealed class CommandAttribute(string name, string description) : Attribute
{
    /// <summary>
    /// Gets the name of the command. That is basically what user is typing in the command line.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the command description that will be used as the command line help message.
    /// </summary>
    public string Description { get; } = description;
}

/// <summary>
/// Marks parameters that contain command line options.
/// </summary>
/// <param name="description">Help message for this option.</param>
/// <param name="valueHint">(Optional) Hint for the option value (e.g. unit, seconds etc.).</param>
/// <param name="alias">(Optional) Single symbol option alias.</param>
[Embedded]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OptionAttribute(string description, string? valueHint = null, char alias = '\0')
    : Attribute
{
    /// <summary>
    /// Gets the option description that will be used as the command line help message.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Gets an option value hint (e.g. unit, seconds etc.).
    /// </summary>
    public string? ValueHint { get; } = valueHint;

    /// <summary>
    /// Gets option alias.
    /// </summary>
    public char? Alias { get; } = alias;
}
""";
}
