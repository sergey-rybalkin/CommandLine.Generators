![Logo](https://media.githubusercontent.com/media/sergey-rybalkin/CommandLine.Generators/refs/heads/main/docs/i/logo.jpg)
# CommandLine.Generators Roslyn Source Generator

[![CI Build](https://github.com/sergey-rybalkin/CommandLine.Generators/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/sergey-rybalkin/CommandLine.Generators/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/CommandLine.Generators)](https://www.nuget.org/packages/CommandLine.Generators)

CommandLine.Generators is a Roslyn incremental source generator for `System.CommandLine` package. It enables declarative command handler definitions by scanning classes marked with attributes and generating registration code scaffolding.

## Features

- Incremental Roslyn source generator
- Attribute-based command metadata
- Constructor parameter inspection for command options
- Simplified parameter type names, such as `int`, `string`, or `FileInfo`
- Embedded attribute definitions for consuming projects

## How it works

The generator injects two attributes into the consuming compilation:

- `CommandAttribute`
- `OptionAttribute`

It then scans for partial classes annotated with `CommandAttribute` and collects option metadata from constructor parameters annotated with `OptionAttribute`.

Based on that information, it generates a second partial class implementation with the following methods:
- `public static Command GetCommandDefinition()` method that creates and returns a `System.CommandLine.Command` instance representing the command definition with all constructor arguments marked with `OptionAttribute` added as `Option<T>` options.
- `public static <ClassName> FromParseResult(ParseResult pr)` method that creates an instance of the command handler class from a `ParseResult` object.
- `static partial void OnCommandDefined(Command cmd);` partial method that can be implemented by the user to customize the generated `Command` instance, for example by adding subcommands or custom validators.
- `static partial void OnCommandCreated(<ClassName> handler, ParseResult pr);` partial method that can be implemented by the user to customize the created command handler instance, for example by performing additional initialization based on the parse result or using dependency injection container to initialize its properties.

When the handler class declares either an `int Execute()` method or a `Task<int> ExecuteAsync(CancellationToken)` method, the generator also wires it into the produced `Command` via `SetAction(...)` so the handler is invoked when the command is run.

Also, the generator emits helper registration method for all matching classes in the target project as an extension method for `RootCommand` class.
```csharp
internal static class RootCommandExtensions
{
    internal static void AddCommandsFromAssembly(this RootCommand root)
    {
        root.Add(global::SampleApp.Commands.MyCommandHandler.GetCommandDefinition());
        // other commands registered here
    }
}
```

## Attributes

### CommandAttribute

Apply this attribute to a class that represents a command handler.

Constructor arguments:

- `name`: command name
- `description`: command description shown in help output

### OptionAttribute

Apply this attribute to constructor parameters that should be treated as command
options.

Constructor arguments:

- `description`: option help text
- `valueHint`: optional hint for the option value
- `alias`: optional single-character alias

## Example

```csharp
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
```

With the example above, the generator discovers:

- command name: `serve`
- command description: `Starts the web server`
- option `port` with simplified type name `int`
- option `root` with simplified type name `DirectoryInfo`

## Generated output

The generator emits a partial class containing the registration method:

```csharp
namespace SampleApp.Commands;

partial class ServeCommand
{
    static partial void OnCommandDefined(Command cmd);

    static partial void OnCommandCreated(ServeCommand handler, ParseResult pr);

    public static Command GetCommandDefinition()
    {
        Option<int> port = new("--port")
        {
            Description = "Port to listen on",
            HelpName = "port",
            Required = true,
        };
        port.Aliases.Add("-p");
        Option<DirectoryInfo> root = new("--root")
        {
            Description = "Root directory",
            HelpName = "path",
            Required = true,
        };
        root.Aliases.Add("-r");

        Command retVal = new("serve", "Starts the web server");
        retVal.Add(port);
        retVal.Add(root);

        retVal.SetAction(parseResult => FromParseResult(parseResult).Execute());

        OnCommandDefined(retVal);

        return retVal;
    }

    public static ServeCommand FromParseResult(ParseResult pr)
    {
        ServeCommand retVal = new(
            pr.GetValue<int>("--port"),
            pr.GetValue<DirectoryInfo>("--root")
        );

        OnCommandCreated(retVal, pr);

        return retVal;
    }
}
```

## Option emission rules

- The long option name is the kebab-cased parameter name (e.g. `rootPath` ? `--root-path`).
- `Description` is always emitted from the `OptionAttribute` description argument.
- `HelpName` is emitted when `valueHint` is provided.
- A single-character `alias` is emitted via `option.Aliases.Add("-x")`.
- `Required = true` is emitted only for non-nullable parameters without a default value.
- Parameters declared as nullable (e.g. `int?`, `string?`) are emitted as `Required = false`.
- Parameters with an explicit default value are emitted as `DefaultValueFactory = static _ => <literal>` and are not required.

## Scope

Only top-level classes declared in a named namespace are processed. Nested types and types declared in the global namespace are ignored by the generator.

## Diagnostics

| Id | Severity | Description |
| --- | --- | --- |
| `CMDGEN001` | Warning | Reported on a `[Command]`-annotated class that declares neither `int Execute()` nor `Task<int> ExecuteAsync(CancellationToken)`. The generator still produces `GetCommandDefinition` and `FromParseResult`, but does not wire `SetAction`. |

Generated file names follow this pattern:

- `{ClassName}_handler.g.cs`
- `RootCommandExtensions.g.cs`

The generator also emits an attributes file:

- `CommandLine.Attributes.g.cs`

## Installation

Add the NuGet package to the project that contains your command handler classes.

Example package reference:

```xml
<ItemGroup>
  <PackageReference Include="CommandLine.Generators"
                    Version="<version>"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Project structure

- `src/CommandLine.Generators` - source generator implementation
- `tests/CommandLine.Generators.Tests` - automated tests
- `tests/Playground` - local experimentation project
- `docs/` - project documentation

## Development

### Build

```powershell
dotnet build
```

### Test

```powershell
dotnet test
```
