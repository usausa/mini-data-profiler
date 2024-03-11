namespace MiniDataProfiler.Exporter.OpenTelemetry;

using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public sealed class OpenTelemetryExporterOption
{
    public bool UseSqlTag { get; set; } = true;

    public bool UseParameterTag { get; set; } = true;

    public bool UseExceptionTag { get; set; } = true;
}

public sealed class OpenTelemetryExporter : IProfileExporter, IDisposable
{
    private readonly OpenTelemetryExporterOption option;

    private readonly ActivitySource activitySource;

    private static readonly AsyncLocal<Activity?> ActivityLocal = new();

    public OpenTelemetryExporter(OpenTelemetryExporterOption option)
    {
        this.option = option;
        activitySource = new ActivitySource(Source.Name, Source.Version);
    }

    public void Dispose()
    {
        activitySource.Dispose();
    }

    public void OnExecuteStart(EventType eventType, DbCommand command)
    {
        var activity = activitySource.StartActivity(eventType.AsString());
        ActivityLocal.Value = activity;

        if (option.UseSqlTag)
        {
            activity?.SetTag("profiler.sql", command.CommandText);
        }

        if (option.UseParameterTag)
        {
            activity?.SetTag("profiler.parameter", MakeParameterText(command));
        }
    }

    public void OnExecuteFinally(EventType eventType, DbCommand command, TimeSpan elapsed)
    {
        ActivityLocal.Value?.Dispose();
        ActivityLocal.Value = null;
    }

    public void OnError(EventType eventType, DbCommand command, Exception ex)
    {
        var activity = ActivityLocal.Value;

        if (option.UseParameterTag)
        {
            activity?.SetTag("profiler.exception", ex);
        }
    }

    private static string MakeParameterText(DbCommand command)
    {
        var handler = new DefaultInterpolatedStringHandler(0, 0, default!, stackalloc char[256]);

        var first = true;
        foreach (DbParameter parameter in command.Parameters)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                handler.AppendLiteral(", ");
            }

            handler.AppendLiteral(parameter.ParameterName);
            handler.AppendLiteral("=");
            handler.AppendFormatted(parameter.Value);
        }

        return handler.ToStringAndClear();
    }
}
