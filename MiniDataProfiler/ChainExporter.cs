namespace MiniDataProfiler;

using System.Data.Common;

public sealed class ChainExporter : IProfileExporter
{
    private readonly IProfileExporter[] exporters;

    public ChainExporter(params IProfileExporter[] exporters)
    {
        this.exporters = exporters;
    }

    public void OnExecuteStart(EventType eventType, DbCommand command)
    {
        foreach (var exporter in exporters)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                exporter.OnExecuteStart(eventType, command);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void OnExecuteFinally(EventType eventType, DbCommand command, TimeSpan elapsed)
    {
        foreach (var exporter in exporters)
        {
            // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
            try
            {
                exporter.OnExecuteFinally(eventType, command, elapsed);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }

    public void OnError(EventType eventType, DbCommand command, Exception ex)
    {
        // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable CA1031
        foreach (var exporter in exporters)
        {
            try
            {
                exporter.OnError(eventType, command, ex);
            }
            catch (Exception)
            {
            }
#pragma warning restore CA1031
        }
    }
}
