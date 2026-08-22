namespace MiniDataProfiler.Mocks;

using System.Data.Common;

internal sealed class RecordingListener : IProfileListener
{
    public List<string> Events { get; } = [];

    public TimeSpan LastExecutedDuration { get; private set; }

    public object? LastScalarResult { get; private set; }

    public int LastNonQueryResult { get; private set; }

    public TimeSpan LastFailedDuration { get; private set; }

    public Exception? LastFailedException { get; private set; }

    public int LastRecordsRead { get; private set; }

    public TimeSpan LastReaderFinishedDuration { get; private set; }

    public EventType LastReaderFinishedEventType { get; private set; }

    public void NonQueryExecuting(in ProfilerExecutingContext context) => Events.Add(nameof(IProfileListener.NonQueryExecuting));

    public void NonQueryExecuted(in ProfilerExecutedContext<int> context)
    {
        Events.Add(nameof(IProfileListener.NonQueryExecuted));
        LastExecutedDuration = context.Duration;
        LastNonQueryResult = context.Result;
    }

    public void ScalarExecuting(in ProfilerExecutingContext context) => Events.Add(nameof(IProfileListener.ScalarExecuting));

    public void ScalarExecuted(in ProfilerExecutedContext<object?> context)
    {
        Events.Add(nameof(IProfileListener.ScalarExecuted));
        LastExecutedDuration = context.Duration;
        LastScalarResult = context.Result;
    }

    public void ReaderExecuting(in ProfilerExecutingContext context) => Events.Add(nameof(IProfileListener.ReaderExecuting));

    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context)
    {
        Events.Add(nameof(IProfileListener.ReaderExecuted));
        LastExecutedDuration = context.Duration;
    }

    public void CommandFailed(in ProfilerFailedContext context)
    {
        Events.Add(nameof(IProfileListener.CommandFailed));
        LastFailedDuration = context.Duration;
        LastFailedException = context.Exception;
    }

    public void CommandFinally(in ProfilerFinallyContext context) => Events.Add(nameof(IProfileListener.CommandFinally));

    public void ReaderFinished(in ProfilerReaderFinishedContext context)
    {
        Events.Add(nameof(IProfileListener.ReaderFinished));
        LastRecordsRead = context.RecordsRead;
        LastReaderFinishedDuration = context.Duration;
        LastReaderFinishedEventType = context.EventType;
    }

    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context) => Events.Add(nameof(IProfileListener.BatchNonQueryExecuting));

    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context)
    {
        Events.Add(nameof(IProfileListener.BatchNonQueryExecuted));
        LastExecutedDuration = context.Duration;
        LastNonQueryResult = context.Result;
    }

    public void BatchReaderExecuting(in BatchProfilerExecutingContext context) => Events.Add(nameof(IProfileListener.BatchReaderExecuting));

    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context)
    {
        Events.Add(nameof(IProfileListener.BatchReaderExecuted));
        LastExecutedDuration = context.Duration;
    }

    public void BatchFailed(in BatchProfilerFailedContext context)
    {
        Events.Add(nameof(IProfileListener.BatchFailed));
        LastFailedDuration = context.Duration;
        LastFailedException = context.Exception;
    }

    public void BatchFinally(in BatchProfilerFinallyContext context) => Events.Add(nameof(IProfileListener.BatchFinally));

    public void BatchReaderFinished(in BatchProfilerReaderFinishedContext context)
    {
        Events.Add(nameof(IProfileListener.BatchReaderFinished));
        LastRecordsRead = context.RecordsRead;
        LastReaderFinishedDuration = context.Duration;
        LastReaderFinishedEventType = context.EventType;
    }
}
