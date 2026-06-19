using System.Collections.Immutable;
using System.CommandLine;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RoslynGeneratorRunResult = Microsoft.CodeAnalysis.GeneratorRunResult;

namespace CommandLine.Generators.Tests.Infrastructure;

/// <summary>
/// Helper for running the source generator end-to-end in unit tests.
/// </summary>
internal static class GeneratorTestHost
{
    private static readonly ImmutableArray<MetadataReference> References = BuildReferences();

    /// <summary>
    /// Compiles the supplied source, runs the generator and returns the result.
    /// </summary>
    /// <param name="source">The C# source to compile.</param>
    /// <returns>The generator run result.</returns>
    public static GeneratorRunResult Run(string source)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            SourceText.From(source, System.Text.Encoding.UTF8),
            new CSharpParseOptions(LanguageVersion.Latest));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "GeneratorTests.Inputs",
            syntaxTrees: [tree],
            references: References,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        CommandLineHandlerGenerator generator = new();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out Compilation outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics);

        GeneratorDriverRunResult runResult = driver.GetRunResult();

        Dictionary<string, string> generatedSources = new(StringComparer.Ordinal);
        foreach (RoslynGeneratorRunResult result in runResult.Results)
        {
            foreach (GeneratedSourceResult sourceResult in result.GeneratedSources)
                generatedSources[sourceResult.HintName] = sourceResult.SourceText.ToString();
        }

        ImmutableArray<Diagnostic> outputDiagnostics = outputCompilation.GetDiagnostics();

        return new GeneratorRunResult(
            generatedSources,
            diagnostics,
            outputDiagnostics);
    }

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        ImmutableArray<MetadataReference>.Builder builder = ImmutableArray.CreateBuilder<MetadataReference>();
        builder.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(CancellationToken).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(Task).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(RootCommand).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(FileInfo).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location));

        AddIfExists(builder, Path.Combine(runtimeDir, "netstandard.dll"));
        AddIfExists(builder, Path.Combine(runtimeDir, "System.Runtime.dll"));
        AddIfExists(builder, Path.Combine(runtimeDir, "System.Collections.dll"));
        AddIfExists(builder, Path.Combine(runtimeDir, "System.IO.dll"));
        AddIfExists(builder, Path.Combine(runtimeDir, "System.IO.FileSystem.dll"));
        AddIfExists(builder, Path.Combine(runtimeDir, "System.Runtime.Extensions.dll"));

        foreach (AssemblyName referenced in typeof(RootCommand).Assembly.GetReferencedAssemblies())
        {
            try
            {
                Assembly loaded = System.Reflection.Assembly.Load(referenced);
                if (!string.IsNullOrEmpty(loaded.Location))
                    builder.Add(MetadataReference.CreateFromFile(loaded.Location));
            }
            catch
            {
                // ignored
            }
        }

        return builder.ToImmutable();
    }

    private static void AddIfExists(ImmutableArray<MetadataReference>.Builder builder, string path)
    {
        if (File.Exists(path))
            builder.Add(MetadataReference.CreateFromFile(path));
    }
}

/// <summary>
/// Aggregated result of running the generator under test.
/// </summary>
/// <param name="GeneratedSources">Map of hint name to generated source text.</param>
/// <param name="GeneratorDiagnostics">Diagnostics reported by the generator itself.</param>
/// <param name="CompilationDiagnostics">Diagnostics from re-compiling the augmented compilation.</param>
internal sealed record GeneratorRunResult(
    Dictionary<string, string> GeneratedSources,
    ImmutableArray<Diagnostic> GeneratorDiagnostics,
    ImmutableArray<Diagnostic> CompilationDiagnostics)
{
    /// <summary>
    /// Gets the first generated command handler source code.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Indicates that the requested operation is invalid.
    /// </exception>
    internal string GetHandlerSource()
    {
        foreach (KeyValuePair<string, string> kvp in GeneratedSources)
        {
            if (kvp.Key.EndsWith("_handler.g.cs", StringComparison.Ordinal))
                return kvp.Value;
        }

        throw new InvalidOperationException("No handler source was generated.");
    }
}
