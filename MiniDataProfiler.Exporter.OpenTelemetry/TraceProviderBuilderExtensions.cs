namespace MiniDataProfiler.Exporter.OpenTelemetry;

using global::OpenTelemetry.Trace;

public static class TraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddMiniDataProfilerInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(SourceName.Name);
        return builder;
    }
}
