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
        activity?.SetTag("db.response.rows_affected", context.Result);
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
        activity?.SetTag("db.response.returned_value", context.Result);
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
        activity?.SetTag("db.response.rows_affected", context.Result.RecordsAffected);
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
            activity.SetTag("exception.message", context.Exception);
        }

        activity.SetTag("db.response.elapsed_ms", (long)context.Duration.TotalMilliseconds);
    }

    public void CommandFinally(in ProfilerFinallyContext context)
    {
        var data = GetAsyncLocalData();
        var activity = data.Activity;
        if (activity is not null)
        {
            activity.SetStatus(data.HasError ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity.Dispose();
            data.Activity = null;
        }
        data.HasError = false;
    }

    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context) =>
        StartBatchActivity(context.EventType, context.Batch);

    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context)
    {
        if (!option.UseResultTag)
        {
            return;
        }

        var data = GetAsyncLocalData();
        var activity = data.Activity;
        activity?.SetTag("db.response.rows_affected", context.Result);
    }

    public void BatchReaderExecuting(in BatchProfilerExecutingContext context) =>
        StartBatchActivity(context.EventType, context.Batch);

    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context)
    {
        if (!option.UseResultTag || (context.Result.RecordsAffected < 0))
        {
            return;
        }

        var data = GetAsyncLocalData();
        var activity = data.Activity;
        activity?.SetTag("db.response.rows_affected", context.Result.RecordsAffected);
    }

    public void BatchFailed(in BatchProfilerFailedContext context)
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
            activity.SetTag("exception.message", context.Exception);
        }

        activity.SetTag("db.response.elapsed_ms", (long)context.Duration.TotalMilliseconds);
    }

    public void BatchFinally(in BatchProfilerFinallyContext context)
    {
        var data = GetAsyncLocalData();
        var activity = data.Activity;
        if (activity is not null)
        {
            activity.SetStatus(data.HasError ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity.Dispose();
            data.Activity = null;
        }
        data.HasError = false;
    }

    private void StartBatchActivity(EventType eventType, DbBatch batch)
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
            activity.SetTag("db.query.text", MakeBatchSqlText(batch));
        }

        activity.Start();
    }

    [SkipLocalsInit]
    private static string MakeBatchSqlText(DbBatch batch)
    {
        var handler = new DefaultInterpolatedStringHandler(0, 0, default!, stackalloc char[512]);

        var first = true;
        foreach (var cmd in batch.BatchCommands)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                handler.AppendLiteral(" | ");
            }

            handler.AppendLiteral(cmd.CommandText);
        }

        return handler.ToStringAndClear();
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
            activity.SetTag("db.query.text", command.CommandText);
        }

        if (option.UseParameterTag)
        {
            activity.SetTag("db.query.parameter", MakeParameterText(command));
        }

        activity.Start();
    }

    [SkipLocalsInit]
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
            switch (parameter.Value)
            {
                case null:
                    handler.AppendLiteral("NULL");
                    break;
                case ISpanFormattable sf:
                    handler.AppendFormatted(sf);
                    break;
                case IFormattable f:
                    handler.AppendLiteral(f.ToString(null, null));
                    break;
                default:
                    handler.AppendLiteral(parameter.Value.ToString() ?? string.Empty);
                    break;
            }
        }

        return handler.ToStringAndClear();
    }
}
