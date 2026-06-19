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

        IncrementalValuesProvider<CommandHandlerModel> partialPipeline = pipeline.Where(
                static model => model.IsPartial
                    && !model.HasMultipleConstructors
                    && !model.IsNestedClass
                    && !model.HasConstructorParametersWithoutOption);

        // Generate source for each command handler class.
        context.RegisterSourceOutput(pipeline, GenerateClassSource);

        // Generate RootCommand extension methods in a separate file.
        context.RegisterSourceOutput(partialPipeline.Collect(), static (context, models) =>
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

    private static void GenerateClassSource(SourceProductionContext context, CommandHandlerModel model)
    {
        if (ReportUnsupportedHandler(context, model))
            return;

        if (!model.HasExecuteMethod)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.MissingExecuteMethod,
                model.Location,
                model.ClassName));
        }

        ReportInvalidOptionAliases(context, model);

        string source = HandlerEmitter.Emit(model);
        context.AddSource($"{model.GetFullClassName()}_handler.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static void ReportInvalidOptionAliases(
        SourceProductionContext context,
        CommandHandlerModel model)
    {
        foreach (CommandParameterModel parameter in model.Parameters.Where(p => p.Alias is not null))
        {
            if (char.IsLetterOrDigit(parameter.Alias!.Value))
                continue;

            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.InvalidOptionAlias,
                model.Location,
                model.ClassName,
                parameter.ParameterName,
                parameter.Alias.Value));
        }
    }

    private static bool ReportUnsupportedHandler(SourceProductionContext context, CommandHandlerModel model)
    {
        if (model.IsNestedClass)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.NestedCommandHandlerNotSupported,
                model.Location,
                model.ClassName));

            return true;
        }

        if (model.HasMultipleConstructors)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.MultipleConstructorsCommandHandler,
                model.Location,
                model.ClassName));

            return true;
        }

        if (!model.IsPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.NonPartialCommandHandler,
                model.Location,
                model.ClassName));

            return true;
        }

        if (model.HasConstructorParametersWithoutOption)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ConstructorParameterWithoutOption,
                model.Location,
                model.ClassName));

            return true;
        }

        return false;
    }
}
