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

        if (option.OutputParameter)
        {
            log.InfoReaderExecutedWithParameter((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Result.RecordsAffected, context.Command.CommandText, MakeParameterText(context.Command));
        }
        else
        {
            log.InfoReaderExecuted((long)context.Duration.TotalMilliseconds, context.EventType.AsString(), context.Result.RecordsAffected, context.Command.CommandText);
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

    [LoggerMessage(Level = LogLevel.Error, Message = "SQL exception. event=[{eventType}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void ErrorExceptionWithParameter(this ILogger logger, string eventType, string sql, string parameter, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "SQL exception. event=[{eventType}], sql=[{sql}]")]
    public static partial void ErrorException(this ILogger logger, string eventType, string sql, Exception ex);
}
