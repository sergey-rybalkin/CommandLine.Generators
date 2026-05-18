using Microsoft.CodeAnalysis;

namespace CommandLine.Generators;

internal static class Diagnostics
{
    /// <summary>
    /// Roslyn diagnostics that should be reported on command handlers that do not define execute method.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingExecuteMethod = new(
        id: "CMDGEN001",
        title: "Missing Execute method",
        messageFormat: MissingExecuteMessage,
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: MissingExecuteDescription);

    private const string MissingExecuteMessage =
        "Type '{0}' must declare either 'int Execute()' or 'Task<int> ExecuteAsync(CancellationToken)'";

    private const string MissingExecuteDescription =
        "Command handlers should declare either 'int Execute()' or "
        + "'Task<int> ExecuteAsync(CancellationToken)' to be invoked from the command line.";
}
