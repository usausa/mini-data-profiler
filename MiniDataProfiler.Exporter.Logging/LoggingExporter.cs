namespace MiniDataProfiler.Exporter.Logging;

using System.Data.Common;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

public sealed class LoggingExporterOption
{
    public bool OutputStartLog { get; set; }

    public bool OutputFinallyLog { get; set; } = true;

    public bool OutputExceptionLog { get; set; }
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

    public void OnExecuteStart(DbCommand command)
    {
        if (!option.OutputStartLog)
        {
            return;
        }

        log.InfoExecute(command.CommandText, MakeParameterText(command));
    }

    public void OnExecuteFinally(DbCommand command, long elapsed)
    {
        if (!option.OutputFinallyLog)
        {
            return;
        }

        log.InfoExecuted(elapsed, command.CommandText, MakeParameterText(command));
    }

    public void OnError(DbCommand command, Exception ex)
    {
        if (!option.OutputExceptionLog)
        {
            return;
        }

        log.ErrorException(ex);
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
    [LoggerMessage(Level = LogLevel.Information, Message = "SQL execute. sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoExecute(this ILogger logger, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Information, Message = "SQL executed. elapsed=[{elapsed}], sql=[{sql}], parameter=[{parameter}]")]
    public static partial void InfoExecuted(this ILogger logger, long elapsed, string sql, string parameter);

    [LoggerMessage(Level = LogLevel.Error, Message = "SQL exception.")]
    public static partial void ErrorException(this ILogger logger, Exception ex);
}
