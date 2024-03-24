namespace MiniDataProfiler;

using System.Data.Common;

public interface IProfileListener
{
    void NonQueryExecuting(in ProfilerExecutingContext context);

    void NonQueryExecuted(in ProfilerExecutedContext<int> context);

    void ScalarExecuting(in ProfilerExecutingContext context);

    void ScalarExecuted(in ProfilerExecutedContext<object?> context);

    void ReaderExecuting(in ProfilerExecutingContext context);

    void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context);

    void CommandFailed(in ProfilerFailedContext context);

    void CommandFinally(in ProfilerFinallyContext context);
}
