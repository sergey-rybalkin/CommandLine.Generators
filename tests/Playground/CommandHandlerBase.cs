using System.Diagnostics;

namespace Playground;

public abstract class CommandHandlerBase
{
    public int Execute()
    {
        long start = Stopwatch.GetTimestamp();

        int retVal = ExecuteInternal();

        TimeSpan stat = Stopwatch.GetElapsedTime(start);

        return retVal;
    }

    protected abstract int ExecuteInternal();
}
