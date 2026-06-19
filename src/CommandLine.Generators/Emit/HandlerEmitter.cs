#pragma warning disable SP2100 // Code line is too long. Some code lines in this file are intentionally long
                               // for readability of the generated code.

using System.Text;
using CommandLine.Generators.Models;

namespace CommandLine.Generators.Emit;

/// <summary>
/// Emits the partial class source for a command handler.
/// </summary>
internal static class HandlerEmitter
{
    private const string SetAsyncActionCode = $"""
        retVal.SetAction(static (parseResult, ct) => {Constants.FromParseResultMethodName}(parseResult).{Constants.ExecuteAsyncMethodName}(ct));
        """;

    private const string SetSyncActionCode = $"""
        retVal.SetAction(static (parseResult) => {Constants.FromParseResultMethodName}(parseResult).{Constants.ExecuteMethodName}());
        """;

    /// <summary>
    /// Builds the source text for the generated partial class.
    /// </summary>
    /// <param name="model">The command handler model.</param>
    /// <returns>The generated C# source.</returns>
    public static string Emit(CommandHandlerModel model)
    {
        CodeWriter code = new(1000 + (model.Parameters.Length * 250));

        if (model.HasAsyncExecute)
        {
            code.AppendFileHeader(
                model.NamespaceName,
                ["System.CommandLine.Parsing", "System.Threading", "System.Threading.Tasks"]);
        }
        else
            code.AppendFileHeader(model.NamespaceName, ["System.CommandLine.Parsing"]);

        code.StartBlock($"partial class {model.ClassName}");
        code.AppendLine($"static partial void {Constants.OnCommandDefinedMethodName}(Command cmd);");
        code.AppendLine();
        code.AppendLine(
            ["static partial void ",
            Constants.OnCommandCreatedMethodName,
            "(",
            model.ClassName,
            " handler, ParseResult pr);"]);
        code.AppendLine();

        EmitGetCommandDefinition(code, model);
        code.AppendLine();
        EmitFromParseResult(code, model);

        code.EndBlock();

        return code.ToString();
    }

    private static void EmitGetCommandDefinition(CodeWriter code, CommandHandlerModel model)
    {
        code.StartBlock($"public static Command {Constants.GetCommandDefinitionMethodName}()");

        for (int i = 0; i < model.Parameters.Length; i++)
            EmitOption(code, model.Parameters[i], i);

        if (model.Parameters.Length > 0)
            code.AppendLine();

        code.AppendLine($"Command retVal = new({Stringify(model.Name)}, {Stringify(model.Description)});");

        for (int i = 0; i < model.Parameters.Length; i++)
            code.AppendLine($"retVal.Add({OptionVariableName(i)});");

        EmitSetAction(code, model);

        code.AppendLine();
        code.AppendLine($"{Constants.OnCommandDefinedMethodName}(retVal);");
        code.AppendLine();
        code.AppendLine("return retVal;");

        code.EndBlock();
    }

    private static void EmitOption(CodeWriter code, CommandParameterModel parameter, int index)
    {
        string variableName = OptionVariableName(index);
        string optionLong = "--" + NameUtilities.ToKebabCase(parameter.ParameterName);
        bool required = !parameter.IsNullable && !parameter.HasDefaultValue;

        code.StartBlock(
            $"Option<{parameter.ParameterTypeName}> {variableName} = new(\"{optionLong}\")");

        code.AppendLine($"Description = {Stringify(parameter.Description)},");
        if (!string.IsNullOrEmpty(parameter.ValueHint))
            code.AppendLine($"HelpName = {Stringify(parameter.ValueHint!)},");

        code.AppendLine($"Required = {required.ToString().ToLowerInvariant()},");
        if (parameter.HasDefaultValue && parameter.DefaultValueLiteral is not null)
            code.AppendLine($"DefaultValueFactory = static _ => {parameter.DefaultValueLiteral},");

        code.EndBlock("};");

        if (parameter.Alias.HasValue)
            code.AppendLine($"{variableName}.Aliases.Add(\"-{parameter.Alias.Value}\");");
    }

    private static string OptionVariableName(int index) => $"option{index + 1}";

    private static void EmitSetAction(CodeWriter code, CommandHandlerModel model)
    {
        if (!model.HasExecuteMethod)
            return;

        code.AppendLine();
        if (model.HasAsyncExecute)
            code.AppendLine(SetAsyncActionCode);
        else
            code.AppendLine(SetSyncActionCode);
    }

    private static void EmitFromParseResult(CodeWriter code, CommandHandlerModel model)
    {
        code.StartBlock(
            $"public static {model.ClassName} {Constants.FromParseResultMethodName}(ParseResult pr)");
        code.AppendLine($"{model.ClassName} retVal = new(");

        if (model.Parameters.Length > 0)
        {
            code.Indent();

            for (int i = 0; i < model.Parameters.Length; i++)
            {
                CommandParameterModel parameter = model.Parameters[i];
                string optionLong = "--" + NameUtilities.ToKebabCase(parameter.ParameterName);
                string postfix = parameter.IsNullable ? "" : "!";
                if (i < model.Parameters.Length - 1)
                    postfix += ",";
                code.AppendLine($"pr.GetValue<{parameter.ParameterTypeName}>(\"{optionLong}\"){postfix}");
            }

            code.Unindent();
        }

        code.AppendLine(");");
        code.AppendLine();
        code.AppendLine($"{Constants.OnCommandCreatedMethodName}(retVal, pr);");
        code.AppendLine();
        code.AppendLine("return retVal;");
        code.EndBlock();
    }

    private static string Stringify(string value)
    {
        StringBuilder sb = new(value.Length + 3);
        sb.Append("@\"");
        sb.Append(value.Replace("\"", "\"\""));
        sb.Append('"');

        return sb.ToString();
    }
}
