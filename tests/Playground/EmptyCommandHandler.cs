using CommandLine.Generators;

namespace Playground;

[Command("empty", "Empty command handler")]
public partial class EmptyCommandHandler
{
    public EmptyCommandHandler(
        [Option("String value", "abc", '1')] string val1,
        [Option("Integer value", "1", '2')] int val2,
        [Option("File path", "/usr/bin/test.txt", '3')] FileInfo val3,
        [Option("Long integer value", "2", 'l')] long lastValue = 10)
    {
        
    }

    public int Execute()
    {
        return 0;
    }
}
