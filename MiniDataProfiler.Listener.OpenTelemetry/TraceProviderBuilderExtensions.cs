namespace MiniDataProfiler.Listener.OpenTelemetry;

using global::OpenTelemetry.Trace;

public static class TraceProviderBuilderExtensions
{
    public static TracerProviderBuilder AddMiniDataProfilerInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(Source.Name);
        return builder;
    }
}
