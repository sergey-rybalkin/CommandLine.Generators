using CommandLine.Generators.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using GeneratorRunResult = CommandLine.Generators.Tests.Infrastructure.GeneratorRunResult;

namespace CommandLine.Generators.Tests;

/// <summary>
/// End-to-end tests for <see cref="CommandLineHandlerGenerator"/>.
/// </summary>
public sealed class CommandLineHandlerGeneratorTests
{
    [Test]
    public void Emits_attributes_file_in_post_initialization()
    {
        GeneratorRunResult result = GeneratorTestHost.Run(string.Empty);

        result.GeneratedSources.ShouldContainKey("CommandLine.Attributes.g.cs");
        result.GeneratedSources["CommandLine.Attributes.g.cs"].ShouldContain("class CommandAttribute");
        result.GeneratedSources["CommandLine.Attributes.g.cs"].ShouldContain("class OptionAttribute");
    }

    [Test]
    public void Generates_handler_partial_with_expected_methods()
    {
        const string source = """
            using System.IO;
            using CommandLine.Generators;

            namespace SampleApp.Commands;

            [Command("serve", "Starts the web server")]
            public partial class ServeCommand
            {
                public ServeCommand(
                    [Option("Port to listen on", "port", 'p')] int port,
                    [Option("Root directory", "path", 'r')] DirectoryInfo root)
                {
                }

                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratedSources.ShouldContainKey("ServeCommand_handler.g.cs");
        string generated = result.GeneratedSources["ServeCommand_handler.g.cs"];
        generated.ShouldContain($"public static Command {Constants.GetCommandDefinitionMethodName}()");
        generated.ShouldContain(
            $"public static ServeCommand {Constants.FromParseResultMethodName}(ParseResult pr)");
        generated.ShouldContain($"partial void {Constants.OnCommandDefinedMethodName}(Command cmd);");
        generated.ShouldContain(
            $"partial void {Constants.OnCommandCreatedMethodName}(ServeCommand handler, ParseResult pr);");

        EnsureNoErrors(result);
    }

    [Test]
    public void Wires_sync_execute_via_set_action_when_execute_method_present()
    {
        const string source = $$"""
            using CommandLine.Generators;

            namespace Sample;

            [Command("run", "")]
            public partial class RunCommand
            {
                public RunCommand() { }
                public int {{Constants.ExecuteMethodName}}() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        string generated = result.GeneratedSources["RunCommand_handler.g.cs"];
        generated.ShouldContain($"{Constants.ExecuteMethodName}()");

        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == "CMDGEN001");
    }

    [Test]
    public void Wires_async_execute_when_execute_async_method_present()
    {
        const string source =
$$"""
using System.Threading;
using System.Threading.Tasks;
using CommandLine.Generators;

namespace Sample;

[Command("run", "")]
public partial class AsyncCommand
{
    public AsyncCommand() { }
    public Task<int> {{Constants.ExecuteAsyncMethodName}}(CancellationToken ct) => Task.FromResult(0);
}
""";

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        string generated = result.GeneratedSources["AsyncCommand_handler.g.cs"];
        generated.ShouldContain($"{Constants.ExecuteAsyncMethodName}(ct)");

        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == "CMDGEN001");
    }

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
        result.GeneratedSources.ShouldContainKey("NoopCommand_handler.g.cs");
        result.GeneratedSources["NoopCommand_handler.g.cs"].ShouldNotContain("SetAction");
    }

    [Test]
    public void Marks_nullable_parameters_as_not_required()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("c", "")]
            public partial class NullableCommand
            {
                public NullableCommand(
                    [Option("p")] int? port,
                    [Option("n")] string? name)
                {
                }

                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        string generated = result.GeneratedSources["NullableCommand_handler.g.cs"];
        generated.ShouldContain("Option<int?> port");
        generated.ShouldContain("Option<string?> name");
        generated.ShouldNotContain("Required = true");
        generated.ShouldNotContain("DefaultValueFactory");
    }

    [Test]
    public void Emits_default_value_factory_for_parameters_with_default_value()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("c", "")]
            public partial class DefaultedCommand
            {
                public DefaultedCommand(
                    [Option("port")] int port = 8080,
                    [Option("enabled")] bool enabled = true)
                {
                }

                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        string generated = result.GeneratedSources["DefaultedCommand_handler.g.cs"];
        generated.ShouldContain("DefaultValueFactory = static _ => 8080");
        generated.ShouldContain("DefaultValueFactory = static _ => true");
        generated.ShouldContain("Required = false");
        EnsureNoErrors(result);
    }

    [Test]
    public void Emits_alias_and_help_name_when_option_attribute_specifies_them()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("c", "")]
            public partial class AliasCommand
            {
                public AliasCommand([Option("Port", "port", 'p')] int port) { }
                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        string generated = result.GeneratedSources["AliasCommand_handler.g.cs"];
        generated.ShouldContain("HelpName = @\"port\"");
        generated.ShouldContain("port.Aliases.Add(\"-p\");");
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
    public void Generates_root_command_extensions_aggregator_with_all_handlers()
    {
        const string source = """
            using CommandLine.Generators;

            namespace A
            {
                [Command("x", "")]
                public partial class X
                {
                    public X() { }
                    public int Execute() => 0;
                }
            }

            namespace B
            {
                [Command("y", "")]
                public partial class Y
                {
                    public Y() { }
                    public int Execute() => 0;
                }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratedSources.ShouldContainKey("RootCommandExtensions.g.cs");
        string aggregator = result.GeneratedSources["RootCommandExtensions.g.cs"];
        aggregator.ShouldContain("namespace CommandLine.Generators;");
        aggregator.ShouldContain("internal static class RootCommandExtensions");
        aggregator.ShouldContain("internal static void AddCommandsFromAssembly(this RootCommand root)");
        aggregator.ShouldContain($"root.Add(global::A.X.{Constants.GetCommandDefinitionMethodName}());");
        aggregator.ShouldContain($"root.Add(global::B.Y.{Constants.GetCommandDefinitionMethodName}());");
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
        result.GeneratedSources.ShouldNotContainKey("NonPartialCommand_handler.g.cs");
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
        result.GeneratedSources.ShouldNotContainKey("MultipleConstructorsCommand_handler.g.cs");
        result.GeneratedSources.ShouldNotContainKey("RootCommandExtensions.g.cs");
    }

    private static void EnsureNoErrors(GeneratorRunResult result)
    {
        IEnumerable<Diagnostic> errors = result.CompilationDiagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error);
        errors.ShouldBeEmpty();
    }
}
