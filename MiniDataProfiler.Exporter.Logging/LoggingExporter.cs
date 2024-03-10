namespace MiniDataProfiler.Exporter.Logging;

using System.Data.Common;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

public sealed class LoggingExporterOption
{
    public bool OutputStartLog { get; set; }

    public bool OutputFinallyLog { get; set; } = true;

    public bool OutputExceptionLog { get; set; }

    public bool OutputParameter { get; set; } = true;
}

public sealed class LoggingExporter : IProfileExporter
{
    private readonly ILogger<LoggingExporter> log;

    private readonly LoggingExporterOption option;

    public LoggingExporter(ILogger<LoggingExporter> log, LoggingExporterOption option)
    {
        this.log = log;
        this.option = option;
    }

    public void OnExecuteStart(EventType eventType, DbCommand command)
    {
        if (!option.OutputStartLog)
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.InfoExecuteWithParameter(eventType.AsString(), command.CommandText, MakeParameterText(command));
        }
        else
        {
            log.InfoExecute(eventType.AsString(), command.CommandText);
        }
    }

    public void OnExecuteFinally(EventType eventType, DbCommand command, TimeSpan elapsed)
    {
        if (!option.OutputFinallyLog)
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.InfoExecutedWithParameter((long)elapsed.TotalMilliseconds, eventType.AsString(), command.CommandText, MakeParameterText(command));
        }
        else
        {
            log.InfoExecuted((long)elapsed.TotalMilliseconds, eventType.AsString(), command.CommandText);
        }
    }

    public void OnError(EventType eventType, DbCommand command, Exception ex)
    {
        if (!option.OutputExceptionLog)
        {
            return;
        }

        if (option.OutputParameter)
        {
            log.ErrorExceptionWithParameter(eventType.AsString(), command.CommandText, ex);
        }
        else
        {
            log.ErrorException(eventType.AsString(), ex);
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

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "SQL execute. event=[{eventType}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoExecuteWithParameter(this ILogger logger, string eventType, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL execute. event=[{eventType}], sql=[{sql}]")]
    public static partial void InfoExecute(this ILogger logger, string eventType, string sql);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoExecutedWithParameter(this ILogger logger, long elapsed, string eventType, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], event=[{eventType}], sql=[{sql}]")]
    public static partial void InfoExecuted(this ILogger logger, long elapsed, string eventType, string sql);

    [LoggerMessage(Level = LogLevel.Error, Message = "SQL exception. sql=[{sql}], parameter=[{parameter}]")]
    public static partial void ErrorExceptionWithParameter(this ILogger logger, string sql, string parameter, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "SQL exception. sql=[{sql}]")]
    public static partial void ErrorException(this ILogger logger, string sql, Exception ex);
}
