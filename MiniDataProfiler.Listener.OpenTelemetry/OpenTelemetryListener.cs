namespace MiniDataProfiler.Listener.OpenTelemetry;

using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public sealed class OpenTelemetryListenerOption
{
    public bool UseSqlTag { get; set; } = true;

    public bool UseParameterTag { get; set; } = true;

    public bool UseResultTag { get; set; } = true;

    public bool UseExceptionTag { get; set; } = true;
}

public sealed class OpenTelemetryListener : IProfileListener, IDisposable
{
#pragma warning disable SA1401 // Fields should be private
    private sealed class AsyncLocalData
    {
        public Activity? Activity;

        public bool HasError;
    }
#pragma warning restore SA1401 // Fields should be private

    private readonly OpenTelemetryListenerOption option;

    private readonly ActivitySource activitySource;

    private static readonly AsyncLocal<AsyncLocalData?> LocalData = new();

    public OpenTelemetryListener(OpenTelemetryListenerOption option)
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

    public void NonQueryExecuting(in ProfilerExecutingContext context) =>
        StartActivity(context.EventType, context.Command);

    public void NonQueryExecuted(in ProfilerExecutedContext<int> context)
    {
        if (!option.UseResultTag)
        {
            return;
        }

        var data = GetAsyncLocalData();
        var activity = data.Activity;
        activity?.SetTag("otel.effect", context.Result);
    }

    public void ScalarExecuting(in ProfilerExecutingContext context) =>
        StartActivity(context.EventType, context.Command);

    public void ScalarExecuted(in ProfilerExecutedContext<object?> context)
    {
        if (!option.UseResultTag)
        {
            return;
        }

        var data = GetAsyncLocalData();
        var activity = data.Activity;
        activity?.SetTag("otel.result", context.Result);
    }

    public void ReaderExecuting(in ProfilerExecutingContext context) =>
        StartActivity(context.EventType, context.Command);

    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context)
    {
        if (!option.UseResultTag || (context.Result.RecordsAffected < 0))
        {
            return;
        }

        var data = GetAsyncLocalData();
        var activity = data.Activity;
        activity?.SetTag("otel.records", context.Result.RecordsAffected);
    }

    public void CommandFailed(in ProfilerFailedContext context)
    {
        var data = GetAsyncLocalData();
        data.HasError = true;

        var activity = data.Activity;
        if (activity is null)
        {
            return;
        }

        if (option.UseExceptionTag)
        {
            activity.SetTag("data.exception", context.Exception);
        }
    }

    public void CommandFinally(in ProfilerFinallyContext context)
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

    private void StartActivity(EventType eventType, DbCommand command)
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
