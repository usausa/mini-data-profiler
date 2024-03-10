namespace MiniDataProfiler.Exporter.OpenTelemetry;

using System.Reflection;

internal static class SourceName
{
    private static readonly AssemblyName AssemblyName = typeof(SourceName).Assembly.GetName();

    public static string Name => AssemblyName.Name!;

    public static string Version => AssemblyName.Version!.ToString();
}
