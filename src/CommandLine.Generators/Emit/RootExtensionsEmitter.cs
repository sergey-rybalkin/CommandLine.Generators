using System.Collections.Immutable;
using CommandLine.Generators.Models;

namespace CommandLine.Generators.Emit;

/// <summary>
/// Emits the RootCommand class extensions methods container class.
/// </summary>
internal static class RootExtensionsEmitter
{
    /// <summary>
    /// Builds the source text for the aggregator class.
    /// </summary>
    /// <param name="models">Collected command handler models.</param>
    /// <returns>The generated C# source.</returns>
    public static string Emit(ImmutableArray<CommandHandlerModel> models)
    {
        CodeWriter code = new(300 + (models.Length * 100));
        code.AppendFileHeader();
        code.StartBlock("internal static class RootCommandExtensions");
        code.StartBlock(
            "internal static void AddCommandsFromAssembly(this RootCommand root, " +
            "System.Action<object>? setupHandler = null)");

        foreach (CommandHandlerModel model in models)
        {
            if (string.IsNullOrEmpty(model.ClassName) || string.IsNullOrEmpty(model.NamespaceName))
                continue;

            code.AppendLine(
                $"root.Add({model.GetSafeClassName()}.{Constants.GetDefinitionMethodName}(setupHandler));");
        }

        code.EndBlock(numBlocks: 2);

        return code.ToString();
    }
}
