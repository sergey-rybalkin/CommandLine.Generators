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

        result.GeneratedSources.ShouldContainKey("SampleApp.Commands.ServeCommand_handler.g.cs");
        string generated = result.GetHandlerSource();
        generated.ShouldContain($"public static Command {Constants.GetDefinitionMethodName}(");
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

        result.GetHandlerSource().ShouldContain($"{Constants.ExecuteMethodName}()");
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

        result.GetHandlerSource().ShouldContain($"{Constants.ExecuteAsyncMethodName}(ct)");

        result.GeneratorDiagnostics.ShouldNotContain(d => d.Id == "CMDGEN001");
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

        string generated = result.GetHandlerSource();
        generated.ShouldContain("Option<int?>");
        generated.ShouldContain("Option<string?>");
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

        string generated = result.GetHandlerSource();
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

        string generated = result.GetHandlerSource();
        generated.ShouldContain("HelpName = @\"port\"");
        generated.ShouldContain("Aliases.Add(\"-p\");");
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
        aggregator.ShouldContain("internal static void AddCommandsFromAssembly(this RootCommand root, " +
            "System.Action<object>? setupHandler = null)");
        aggregator.ShouldContain(
            $"root.Add(global::A.X.{Constants.GetDefinitionMethodName}(setupHandler));");
        aggregator.ShouldContain(
            $"root.Add(global::B.Y.{Constants.GetDefinitionMethodName}(setupHandler));");
    }

    [Test]
    public void Wires_command_hook_into_sync_action_before_execute()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Sample;

            [Command("run", "")]
            public partial class HookedCommand
            {
                public HookedCommand() { }
                public int Execute() => 0;
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        string generated = result.GetHandlerSource();
        generated.ShouldContain($"public static Command {Constants.GetDefinitionMethodName}(");
        generated.ShouldContain($".{Constants.ExecuteMethodName}();");
        EnsureNoErrors(result);
    }

    [Test]
    public void Wires_command_hook_into_async_action_before_execute()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;
            using CommandLine.Generators;

            namespace Sample;

            [Command("run", "")]
            public partial class HookedAsyncCommand
            {
                public HookedAsyncCommand() { }
                public Task<int> ExecuteAsync(CancellationToken ct) => Task.FromResult(0);
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        string generated = result.GetHandlerSource();
        generated.ShouldContain("setupHandler(cmd);");
        generated.ShouldContain($"return cmd.{Constants.ExecuteAsyncMethodName}(ct);");
        EnsureNoErrors(result);
    }

    [Test]
    public void Generates_unique_files_for_classes_with_identical_names_in_different_namespaces()
    {
        const string source = """
            using CommandLine.Generators;

            namespace Ns1
            {
                [Command("cmd1", "")]
                public partial class RunCommand
                {
                    public RunCommand() { }
                    public int Execute() => 0;
                }
            }

            namespace Ns2
            {
                [Command("cmd2", "")]
                public partial class RunCommand
                {
                    public RunCommand() { }
                    public int Execute() => 0;
                }
            }
            """;

        GeneratorRunResult result = GeneratorTestHost.Run(source);

        result.GeneratedSources.ShouldContainKey("Ns1.RunCommand_handler.g.cs");
        result.GeneratedSources.ShouldContainKey("Ns2.RunCommand_handler.g.cs");
        EnsureNoErrors(result);
    }

    private static void EnsureNoErrors(GeneratorRunResult result)
    {
        IEnumerable<Diagnostic> errors = result.CompilationDiagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error);
        errors.ShouldBeEmpty();
    }
}
