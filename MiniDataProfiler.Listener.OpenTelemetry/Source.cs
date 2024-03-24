namespace MiniDataProfiler.Listener.OpenTelemetry;

using System.Reflection;

internal static class Source
{
    private static readonly AssemblyName AssemblyName = typeof(Source).Assembly.GetName();

    public static string Name => AssemblyName.Name!;

    public static string Version => AssemblyName.Version!.ToString();
}
