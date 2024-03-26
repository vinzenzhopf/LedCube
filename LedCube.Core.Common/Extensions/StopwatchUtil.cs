using System.Diagnostics;

namespace LedCube.Core.Common.Extensions;

public static class StopwatchUtil
{
    public static double TicksToSeconds(long ticks)
    {
        return (double)ticks / Stopwatch.Frequency;
    } 
    
    public static double TicksToMilliseconds(long ticks)
    {
        return (double)ticks / Stopwatch.Frequency * 1000;
    }
    
    public static double TicksToMicroseconds(long ticks)
    {
        return (double)ticks / Stopwatch.Frequency * 1000000;
    }
}