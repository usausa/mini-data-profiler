namespace MiniDataProfiler;

using System.Diagnostics;
using System.Runtime.CompilerServices;

internal static class ProfileHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetTimeStamp() => Stopwatch.GetTimestamp();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetElapsed(long start) => (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
}
