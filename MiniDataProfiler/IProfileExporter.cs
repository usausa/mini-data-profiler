namespace MiniDataProfiler;

using System.Data.Common;

public interface IProfileExporter
{
    void OnExecuteStart(EventType eventType, DbCommand command);

    void OnExecuteFinally(EventType eventType, DbCommand command, TimeSpan elapsed);

    void OnError(EventType eventType, DbCommand command, Exception ex);
}
