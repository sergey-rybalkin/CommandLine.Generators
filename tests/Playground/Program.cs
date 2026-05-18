using System.CommandLine;
using CommandLine.Generators;

namespace Playground;

/// <summary>
/// Contains application entry point.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry-point for this application.
    /// </summary>
    /// <param name="args">An array of command-line argument strings.</param>
    public static int Main(string[] args)
    {
        RootCommand rootCommand = new("Sample app for System.CommandLine");
        rootCommand.AddCommandsFromAssembly();

        ParseResult parseResult = rootCommand.Parse(args);

        return parseResult.Invoke();
    }
}
