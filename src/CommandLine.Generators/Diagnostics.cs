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

    internal const string MissingExecuteMethodId = "CMDGEN001";

    internal const string NonPartialCommandHandlerId = "CMDGEN002";

    internal const string MultipleConstructorsCommandHandlerId = "CMDGEN003";

    private const string MissingExecuteMessage =
        "Type '{0}' must declare either 'int Execute()' or 'Task<int> ExecuteAsync(CancellationToken)'";

    private const string MissingExecuteDescription =
        "Command handlers should declare either 'int Execute()' or "
        + "'Task<int> ExecuteAsync(CancellationToken)' to be invoked from the command line.";

    private const string NonPartialCommandHandlerMessage =
        "Type '{0}' is marked with [Command] but is not declared as partial. "
        + "Source generator requires partial classes to emit code.";

    private const string NonPartialCommandHandlerDescription =
        "Command handler classes must be declared as partial so that the source generator "
        + "can emit the second partial class file with registration code.";

    private const string MultipleConstructorsCommandHandlerMessage =
        "Type '{0}' is marked with [Command] but declares multiple constructors. "
        + "Source generator requires a single constructor to resolve command options.";

    private const string MultipleConstructorsCommandHandlerDescription =
        "Command handler classes must declare a single constructor so that the source generator "
        + "can resolve constructor parameters as command options unambiguously.";
}
