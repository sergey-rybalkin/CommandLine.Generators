using System.CommandLine;
using CommandLine.Generators;

namespace Playground;

[Command("demo", "Demo command handler")]
public partial class DemoCommandHandler : CommandHandlerBase
{
    private readonly string _val1;
    private readonly int _val2;
    private readonly FileInfo _val3;
    private readonly long _lastValue;

    public DemoCommandHandler(
        [Option("String value", "abc", '1')] string val1,
        [Option("Integer value", "1", '2')] int val2,
        [Option("File path", "/usr/bin/test.txt", '3')] FileInfo val3,
        [Option("Long integer value", "2", 'l')] long lastValue = 10)
    {
        _val1 = val1;
        _val2 = val2;
        _val3 = val3;
        _lastValue = lastValue;
    }

    protected override int ExecuteInternal()
    {
        Console.WriteLine($"String value: {_val1}");
        Console.WriteLine($"Integer value: {_val2}");
        Console.WriteLine($"File path: {_val3}");
        Console.WriteLine($"Long integer value: {_lastValue}");

        return 0;
    }
}
