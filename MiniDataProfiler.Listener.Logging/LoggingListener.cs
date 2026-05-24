namespace MiniDataProfiler.Listener.Logging;

using System.Data.Common;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

public sealed class LoggingListenerOption
{
    public bool OutputStartLog { get; set; }

    public bool OutputFinallyLog { get; set; } = true;

    public bool OutputExceptionLog { get; set; }

    public bool OutputParameter { get; set; } = true;

    public TimeSpan ElapsedThreshold { get; set; } = TimeSpan.Zero;
}

public sealed class LoggingListener : IProfileListener
{
    private readonly ILogger<LoggingListener> log;

    private readonly LoggingListenerOption option;

    public LoggingListener(ILogger<LoggingListener> log, LoggingListenerOption option)
    {
        this.log = log;
        this.option = option;
    }

    public void NonQueryExecuting(in ProfilerExecutingContext context)
    {
        if (!option.OutputStartLog)
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.InfoExecutingWithParameter(context.EventType.AsString(), context.Command.CommandText, MakeParameterText(context.Command));
        }
        else
        {
            log.InfoExecuting(context.EventType.AsString(), context.Command.CommandText);
        }
    }

    public void NonQueryExecuted(in ProfilerExecutedContext<int> context)
    {
        if (!option.OutputFinallyLog || (context.Duration < option.ElapsedThreshold))
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.InfoNonQueryExecutedWithParameter((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Result, context.Command.CommandText, MakeParameterText(context.Command));
        }
        else
        {
            log.InfoNonQueryExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Result, context.Command.CommandText);
        }
    }

    public void ScalarExecuting(in ProfilerExecutingContext context)
    {
        if (!option.OutputStartLog)
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.InfoExecutingWithParameter(context.EventType.AsString(), context.Command.CommandText, MakeParameterText(context.Command));
        }
        else
        {
            log.InfoExecuting(context.EventType.AsString(), context.Command.CommandText);
        }
    }

    public void ScalarExecuted(in ProfilerExecutedContext<object?> context)
    {
        if (!option.OutputFinallyLog || (context.Duration < option.ElapsedThreshold))
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.InfoScalarExecutedWithParameter((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Result, context.Command.CommandText, MakeParameterText(context.Command));
        }
        else
        {
            log.InfoScalarExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Result, context.Command.CommandText);
        }
    }

    public void ReaderExecuting(in ProfilerExecutingContext context)
    {
        if (!option.OutputStartLog)
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.InfoExecutingWithParameter(context.EventType.AsString(), context.Command.CommandText, MakeParameterText(context.Command));
        }
        else
        {
            log.InfoExecuting(context.EventType.AsString(), context.Command.CommandText);
        }
    }

    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context)
    {
        if (!option.OutputFinallyLog || (context.Duration < option.ElapsedThreshold))
        {
            return;
        }

        var records = context.Result.RecordsAffected;
        if (records < 0)
        {
            if (option.OutputParameter)
            {
                log.InfoReaderExecutedWithParameter((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Command.CommandText, MakeParameterText(context.Command));
            }
            else
            {
                log.InfoReaderExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Command.CommandText);
            }
        }
        else
        {
            if (option.OutputParameter)
            {
                log.InfoReaderExecutedWithParameter((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), records, context.Command.CommandText, MakeParameterText(context.Command));
            }
            else
            {
                log.InfoReaderExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), records, context.Command.CommandText);
            }
        }
    }

    public void CommandFailed(in ProfilerFailedContext context)
    {
        if (!option.OutputExceptionLog)
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.ErrorExceptionWithParameter(context.EventType.AsString(), context.Command.CommandText, MakeParameterText(context.Command), context.Exception);
        }
        else
        {
            log.ErrorException(context.EventType.AsString(), context.Command.CommandText, context.Exception);
        }
    }

    public void CommandFinally(in ProfilerFinallyContext context)
    {
        // Do Nothing
    }

    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context)
    {
        if (!option.OutputStartLog)
        {
            return;
        }

        log.InfoExecuting(context.EventType.AsString(), MakeBatchSqlText(context.Batch));
    }

    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context)
    {
        if (!option.OutputFinallyLog || (context.Duration < option.ElapsedThreshold))
        {
            return;
        }

        log.InfoNonQueryExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Result, MakeBatchSqlText(context.Batch));
    }

    public void BatchReaderExecuting(in BatchProfilerExecutingContext context)
    {
        if (!option.OutputStartLog)
        {
            return;
        }

        log.InfoExecuting(context.EventType.AsString(), MakeBatchSqlText(context.Batch));
    }

    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context)
    {
        if (!option.OutputFinallyLog || (context.Duration < option.ElapsedThreshold))
        {
            return;
        }

        var records = context.Result.RecordsAffected;
        if (records < 0)
        {
            log.InfoReaderExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), MakeBatchSqlText(context.Batch));
        }
        else
        {
            log.InfoReaderExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), records, MakeBatchSqlText(context.Batch));
        }
    }

    public void BatchFailed(in BatchProfilerFailedContext context)
    {
        if (!option.OutputExceptionLog)
        {
            return;
        }

        log.ErrorException(context.EventType.AsString(), MakeBatchSqlText(context.Batch), context.Exception);
    }

    public void BatchFinally(in BatchProfilerFinallyContext context)
    {
        // Do Nothing
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

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "SQL execute. event=[{eventType}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoExecutingWithParameter(this ILogger logger, string eventType, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL execute. event=[{eventType}], sql=[{sql}]")]
    public static partial void InfoExecuting(this ILogger logger, string eventType, string sql);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], effect=[{effect}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoNonQueryExecutedWithParameter(this ILogger logger, long elapsed, string eventType, int effect, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], effect=[{effect}], sql=[{sql}]")]
    public static partial void InfoNonQueryExecuted(this ILogger logger, long elapsed, string eventType, int effect, string sql);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], result=[{result}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoScalarExecutedWithParameter(this ILogger logger, long elapsed, string eventType, object? result, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], result=[{result}], sql=[{sql}]")]
    public static partial void InfoScalarExecuted(this ILogger logger, long elapsed, string eventType, object? result, string sql);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], records=[{records}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoReaderExecutedWithParameter(this ILogger logger, long elapsed, string eventType, int records, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], records=[{records}], sql=[{sql}]")]
    public static partial void InfoReaderExecuted(this ILogger logger, long elapsed, string eventType, int records, string sql);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoReaderExecutedWithParameter(this ILogger logger, long elapsed, string eventType, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], sql=[{sql}]")]
    public static partial void InfoReaderExecuted(this ILogger logger, long elapsed, string eventType, string sql);

    [LoggerMessage(Level = LogLevel.Error, Message = "SQL exception. event=[{eventType}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void ErrorExceptionWithParameter(this ILogger logger, string eventType, string sql, string parameter, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "SQL exception. event=[{eventType}], sql=[{sql}]")]
    public static partial void ErrorException(this ILogger logger, string eventType, string sql, Exception ex);
}
