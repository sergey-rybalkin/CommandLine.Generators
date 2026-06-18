using Microsoft.CodeAnalysis;

namespace CommandLine.Generators;

internal static class Diagnostics
{
    /// <summary>
    /// Roslyn diagnostics that should be reported on command handlers that do not define execute method.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingExecuteMethod = new(
        id: MissingExecuteMethodId,
        title: "Missing Execute method",
        messageFormat: MissingExecuteMessage,
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: MissingExecuteDescription);

    /// <summary>
    /// Roslyn diagnostics that should be reported on command handlers that are not declared as partial.
    /// </summary>
    public static readonly DiagnosticDescriptor NonPartialCommandHandler = new(
        id: NonPartialCommandHandlerId,
        title: "Non-partial command handler",
        messageFormat: NonPartialCommandHandlerMessage,
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: NonPartialCommandHandlerDescription);

    /// <summary>
    /// Roslyn diagnostics that should be reported on command handlers that declare multiple constructors.
    /// </summary>
    public static readonly DiagnosticDescriptor MultipleConstructorsCommandHandler = new(
        id: MultipleConstructorsCommandHandlerId,
        title: "Multiple constructors command handler",
        messageFormat: MultipleConstructorsCommandHandlerMessage,
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: MultipleConstructorsCommandHandlerDescription);

    /// <summary>
    /// Roslyn diagnostics that should be reported on nested command handlers that are not supported.
    /// </summary>
    public static readonly DiagnosticDescriptor NestedCommandHandlerNotSupported = new(
        id: NestedCommandHandlerNotSupportedId,
        title: "Nested command handlers are not supported",
        messageFormat: NestedCommandHandlerNotSupportedMessage,
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: NestedCommandHandlerNotSupportedDescription);

    /// <summary>
    /// Roslyn diagnostics that should be reported on command handlers that have constructor parameters
    /// without OptionAttribute.
    /// </summary>
    public static readonly DiagnosticDescriptor ConstructorParameterWithoutOption = new(
        id: ConstructorParameterWithoutOptionId,
        title: "Constructor parameter is missing Option attribute",
        messageFormat: ConstructorParameterWithoutOptionMessage,
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: ConstructorParameterWithoutOptionDescription);

    internal const string MissingExecuteMethodId = "CMDGEN001";

    internal const string NonPartialCommandHandlerId = "CMDGEN002";

    internal const string MultipleConstructorsCommandHandlerId = "CMDGEN003";

    internal const string NestedCommandHandlerNotSupportedId = "CMDGEN004";

    internal const string ConstructorParameterWithoutOptionId = "CMDGEN005";

    private const string MissingExecuteMessage =
        "Type '{0}' must declare or inherit public/internal 'int Execute()' or " +
        "public/internal 'Task<int> ExecuteAsync(CancellationToken)'";

    private const string MissingExecuteDescription =
        "Command handlers should declare or inherit public/internal 'int Execute()' or " +
        "public/internal 'Task<int> ExecuteAsync(CancellationToken)' to be invoked from the command line.";

    private const string NonPartialCommandHandlerMessage =
        "Type '{0}' is marked with [Command] but is not declared as partial. " +
        "Source generator requires partial classes to emit code.";

    private const string NonPartialCommandHandlerDescription =
        "Command handler classes must be declared as partial so that the source generator " +
        "can emit the second partial class file with registration code.";

    private const string MultipleConstructorsCommandHandlerMessage =
        "Type '{0}' is marked with [Command] but declares multiple constructors. " +
        "Source generator requires a single constructor to resolve command options.";

    private const string MultipleConstructorsCommandHandlerDescription =
        "Command handler classes must declare a single constructor so that the source generator " +
        "can resolve constructor parameters as command options unambiguously.";

    private const string NestedCommandHandlerNotSupportedMessage =
        "Type '{0}' is marked with [Command] but is nested. " +
        "Source generator supports only top-level command handler classes.";

    private const string NestedCommandHandlerNotSupportedDescription =
        "Command handler classes marked with [Command] must be top-level, " +
        "non-nested classes so that the source generator can emit registration code.";

    private const string ConstructorParameterWithoutOptionMessage =
        "Type '{0}' has constructor parameters without [Option]. " +
        "Source generator requires all constructor parameters to be command options.";

    private const string ConstructorParameterWithoutOptionDescription =
        "Command handler constructor parameters must be marked with [Option] so that values " +
        "can be provided from the command line.";
}
