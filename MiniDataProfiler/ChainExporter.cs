namespace MiniDataProfiler;

using System.Data.Common;

public sealed class ChainExporter : IProfileExporter
{
    private readonly IProfileExporter[] exporters;

    public ChainExporter(params IProfileExporter[] exporters)
    {
        this.exporters = exporters;
    }

    public void OnExecuteStart(DbCommand command)
    {
        foreach (var exporter in exporters)
        {
            exporter.OnExecuteStart(command);
        }
    }

    public void OnExecuteFinally(DbCommand command, TimeSpan elapsed)
    {
        foreach (var exporter in exporters)
        {
            exporter.OnExecuteFinally(command, elapsed);
        }
    }

    public void OnError(DbCommand command, Exception ex)
    {
        foreach (var exporter in exporters)
        {
            exporter.OnError(command, ex);
        }
    }
}
