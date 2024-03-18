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
#pragma warning disable SA1401 // Fields should be private
    private sealed class AsyncLocalData
    {
        public Activity? Activity;

        public bool HasError;
    }
#pragma warning restore SA1401 // Fields should be private

    private readonly OpenTelemetryExporterOption option;

    private readonly ActivitySource activitySource;

    private static readonly AsyncLocal<AsyncLocalData?> LocalData = new();

    public OpenTelemetryExporter(OpenTelemetryExporterOption option)
    {
        this.option = option;
        activitySource = new ActivitySource(Source.Name, Source.Version);
    }

    public void Dispose()
    {
        activitySource.Dispose();
    }

    private static AsyncLocalData GetAsyncLocalData()
    {
        var data = LocalData.Value;
        if (data is null)
        {
            data = new AsyncLocalData();
            LocalData.Value = data;
        }

        return data;
    }

    public void OnExecuteStart(EventType eventType, DbCommand command)
    {
        var activity = activitySource.CreateActivity(eventType.AsString(), ActivityKind.Client);

        var data = GetAsyncLocalData();
        data.Activity = activity;
        data.HasError = false;

        if (activity is null)
        {
            return;
        }

        if (option.UseSqlTag)
        {
            activity.SetTag("data.sql", command.CommandText);
        }

        if (option.UseParameterTag)
        {
            activity.SetTag("data.parameter", MakeParameterText(command));
        }

        activity.Start();
    }

    public void OnExecuteFinally(EventType eventType, DbCommand command, TimeSpan elapsed)
    {
        var data = GetAsyncLocalData();
        var activity = data.Activity;
        if (activity is not null)
        {
            activity.SetTag("otel.status_code", data.HasError ? "ERROR" : "OK");
            activity.Dispose();
            data.Activity = null;
        }
        data.HasError = false;
    }

    public void OnError(EventType eventType, DbCommand command, Exception ex)
    {
        var data = GetAsyncLocalData();
        data.HasError = true;

        var activity = data.Activity;
        if (activity is null)
        {
            return;
        }

        if (option.UseParameterTag)
        {
            activity.SetTag("data.exception", ex);
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
