using System.Text;
using CommandLine.Generators.Emit;
using CommandLine.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CommandLine.Generators;

/// <summary>
/// Source code generator for System.CommandLine library that registers declarative command handlers in
/// CLI pipeline.
/// </summary>
[Generator]
public class CommandLineHandlerGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Called to initialize the generator and register generation steps via callbacks on the
    /// <paramref name="context" />.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IncrementalGeneratorInitializationContext" /> to register callbacks on.
    /// </param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddEmbeddedAttributeDefinition();
            postInitializationContext.AddSource(
                Constants.AttributesDefinitionFileName,
                SourceText.From(Constants.AttributesDefinition, Encoding.UTF8));
        });

        IncrementalValuesProvider<CommandHandlerModel> pipeline =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: Constants.MetadataAttributeName,
                predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
                transform: TransformSyntax)
            .Where(static model => !string.IsNullOrEmpty(model.ClassName)
                && !string.IsNullOrEmpty(model.NamespaceName));

        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            if (!model.HasExecuteMethod)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.MissingExecuteMethod,
                    model.Location,
                    model.ClassName));
            }

            string source = HandlerEmitter.Emit(model);
            context.AddSource($"{model.ClassName}_handler.g.cs", SourceText.From(source, Encoding.UTF8));
        });

        context.RegisterSourceOutput(pipeline.Collect(), static (context, models) =>
        {
            if (models.IsDefaultOrEmpty)
                return;

            string source = RootExtensionsEmitter.Emit(models);
            context.AddSource(Constants.RootExtensionsFileName, SourceText.From(source, Encoding.UTF8));
        });
    }

    private static CommandHandlerModel TransformSyntax(
        GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        return CommandHandlerModelBuilder.FromSyntaxContext(context, ct);
    }
}
