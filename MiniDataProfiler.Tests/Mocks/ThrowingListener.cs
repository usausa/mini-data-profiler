namespace MiniDataProfiler.Mocks;

using System.Data.Common;

internal sealed class ThrowingListener : IProfileListener
{
    public void NonQueryExecuting(in ProfilerExecutingContext context) => throw new InvalidOperationException("ThrowingListener");
    public void NonQueryExecuted(in ProfilerExecutedContext<int> context) => throw new InvalidOperationException("ThrowingListener");
    public void ScalarExecuting(in ProfilerExecutingContext context) => throw new InvalidOperationException("ThrowingListener");
    public void ScalarExecuted(in ProfilerExecutedContext<object?> context) => throw new InvalidOperationException("ThrowingListener");
    public void ReaderExecuting(in ProfilerExecutingContext context) => throw new InvalidOperationException("ThrowingListener");
    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context) => throw new InvalidOperationException("ThrowingListener");
    public void CommandFailed(in ProfilerFailedContext context) => throw new InvalidOperationException("ThrowingListener");
    public void CommandFinally(in ProfilerFinallyContext context) => throw new InvalidOperationException("ThrowingListener");
    public void ReaderFinished(in ProfilerReaderFinishedContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchReaderExecuting(in BatchProfilerExecutingContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchFailed(in BatchProfilerFailedContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchFinally(in BatchProfilerFinallyContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchReaderFinished(in BatchProfilerReaderFinishedContext context) => throw new InvalidOperationException("ThrowingListener");
}
