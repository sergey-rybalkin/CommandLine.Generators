using CommandLine.Generators.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using GeneratorRunResult = CommandLine.Generators.Tests.Infrastructure.GeneratorRunResult;

namespace CommandLine.Generators.Tests;

/// <summary>
/// End-to-end tests for inherited command handler execute methods.
/// </summary>
public sealed class CommandLineHandlerGeneratorInheritanceTests
{
    [Test]
    public void Wires_inherited_sync_execute_via_set_action()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            public class CommandBase
            {
                public int Execute() => 0;
            }

            [Command("run", "")]
            public partial class RunCommand : CommandBase
            {
                public RunCommand() { }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GetHandlerSource()
            .ShouldContain($"{Constants.FromParseResultMethodName}(parseResult).Execute()");
        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        EnsureNoErrors(result);
    }

    [Test]
    public void Wires_inherited_async_execute_via_set_action()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;
            using CommandLine.Generators;

            namespace Sample;

            public class CommandBase
            {
                public Task<int> ExecuteAsync(CancellationToken ct) => Task.FromResult(0);
            }

            [Command("run", "")]
            public partial class RunCommand : CommandBase
            {
                public RunCommand() { }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GetHandlerSource()
            .ShouldContain($"{Constants.FromParseResultMethodName}(parseResult).ExecuteAsync(ct)");
        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        EnsureNoErrors(result);
    }

    [Test]
    public void Accepts_internal_inherited_execute_method()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            internal class CommandBase
            {
                internal int Execute() => 0;
            }

            [Command("run", "")]
            internal partial class RunCommand : CommandBase
            {
                public RunCommand() { }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GetHandlerSource().ShouldContain(".Execute()");
        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        EnsureNoErrors(result);
    }

    [Test]
    public void Reports_when_execute_method_is_not_public_or_internal()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("run", "")]
            public partial class RunCommand
            {
                public RunCommand() { }
                private int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        result.GetHandlerSource().ShouldNotContain("SetAction");
    }

    [Test]
    public void Reports_when_inherited_execute_method_is_not_public_or_internal()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            public class CommandBase
            {
                protected int Execute() => 0;
            }

            [Command("run", "")]
            public partial class RunCommand : CommandBase
            {
                public RunCommand() { }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        result.GetHandlerSource().ShouldNotContain("SetAction");
    }

    private static void EnsureNoErrors(GeneratorRunResult result)
    {
        IEnumerable<Diagnostic> errors = result.CompilationDiagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error);
        errors.ShouldBeEmpty();
    }
}
