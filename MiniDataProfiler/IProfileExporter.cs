namespace MiniDataProfiler;

using System.Data.Common;

public interface IProfileExporter
{
    void OnExecuteStart(DbCommand command);

    void OnExecuteFinally(DbCommand command, long elapsed);

    void OnError(DbCommand command, Exception ex);
}
