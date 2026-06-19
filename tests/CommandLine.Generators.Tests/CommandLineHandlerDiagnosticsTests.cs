using CommandLine.Generators.Tests.Infrastructure;
using GeneratorRunResult = CommandLine.Generators.Tests.Infrastructure.GeneratorRunResult;

namespace CommandLine.Generators.Tests;

/// <summary>
/// Tests for diagnostics emitted by <see cref="CommandLineHandlerGenerator"/>.
/// </summary>
public sealed class CommandLineHandlerDiagnosticsTests
{
    [Test]
    public void Reports_diagnostics_when_no_execute_method_defined_but_still_emits_partial()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("noop", "")]
            public partial class NoopCommand
            {
                public NoopCommand() { }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        result.GetHandlerSource().ShouldNotContain("SetAction");
    }

    [Test]
    public void Reports_nested_classes()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            public partial class Outer
            {
                [Command("nested", "")]
                public partial class NestedCommand
                {
                    public NestedCommand() { }
                    public int Execute() => 0;
                }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratedSources.ShouldNotContainKey("GlobalCommand_handler.g.cs");
        result.GeneratedSources.ShouldNotContainKey("NestedCommand_handler.g.cs");
        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        result.GeneratorDiagnostics.ShouldContain(
            d => d.Id == Diagnostics.NestedCommandHandlerNotSupportedId);
    }

    [Test]
    public void Reports_when_class_is_not_partial()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("run", "Runs the app")]
            public class NonPartialCommand
            {
                public NonPartialCommand() { }
                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldContain(d => d.Id == Diagnostics.NonPartialCommandHandlerId);
        result.GeneratedSources.ShouldNotContainKey("Sample.NonPartialCommand_handler.g.cs");
    }

    [Test]
    public void Reports_when_class_has_multiple_constructors()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("run", "Runs the app")]
            public partial class MultipleConstructorsCommand
            {
                public MultipleConstructorsCommand() { }
                public MultipleConstructorsCommand([Option("Port")] int port) { }
                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldContain(
            d => d.Id == Diagnostics.MultipleConstructorsCommandHandlerId);
        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        result.GeneratedSources.ShouldNotContainKey("Sample.MultipleConstructorsCommand_handler.g.cs");
        result.GeneratedSources.ShouldNotContainKey("RootCommandExtensions.g.cs");
    }

    [Test]
    public void Reports_when_constructor_parameter_has_no_option_attribute()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("demo", "Demo command handler")]
            public partial class DemoCommandHandler
            {
                public DemoCommandHandler(
                    [Option("String value", "abc", '1')] string val1,
                    int val2)
                {
                }

                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldContain(
            d => d.Id == Diagnostics.ConstructorParameterWithoutOptionId);
        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == Diagnostics.MissingExecuteMethodId);
        result.GeneratedSources.ShouldNotContainKey("Sample.DemoCommandHandler_handler.g.cs");
        result.GeneratedSources.ShouldNotContainKey("RootCommandExtensions.g.cs");
    }

    [Test]
    public void Reports_non_alphanumeric_option_alias_but_still_generates_code()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("c", "")]
            public partial class AliasCommand
            {
                public AliasCommand([Option("Port", "port", '#')] int port) { }
                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldContain(d => d.Id == Diagnostics.InvalidOptionAliasId);
        string generated = result.GetHandlerSource();
        generated.ShouldContain("Aliases.Add(\"-#\");");
    }

    [Test]
    public void Does_not_report_invalid_alias_for_alphanumeric_aliases()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("c", "")]
            public partial class AliasCommand
            {
                public AliasCommand(
                    [Option("Port", "port", 'p')] int port,
                    [Option("Verbose", "verbose", '1')] bool verbose)
                {
                }

                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == Diagnostics.InvalidOptionAliasId);
    }
}
