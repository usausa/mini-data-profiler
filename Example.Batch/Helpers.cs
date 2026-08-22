namespace Example.Batch;

using System.Data.Common;

using MiniDataProfiler;

internal sealed class RecordingListener : IProfileListener
{
    public List<string> Events { get; } = [];

    public int LastRecordsRead { get; private set; }

    public void Clear()
    {
        Events.Clear();
        LastRecordsRead = 0;
    }

    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context) => Events.Add(nameof(BatchNonQueryExecuting));

    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context) => Events.Add(nameof(BatchNonQueryExecuted));

    public void BatchReaderExecuting(in BatchProfilerExecutingContext context) => Events.Add(nameof(BatchReaderExecuting));

    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context) => Events.Add(nameof(BatchReaderExecuted));

    public void BatchFailed(in BatchProfilerFailedContext context) => Events.Add(nameof(BatchFailed));

    public void BatchFinally(in BatchProfilerFinallyContext context) => Events.Add(nameof(BatchFinally));

    public void BatchReaderFinished(in BatchProfilerReaderFinishedContext context)
    {
        LastRecordsRead = context.RecordsRead;
        Events.Add(nameof(BatchReaderFinished));
    }

    public void NonQueryExecuting(in ProfilerExecutingContext context)
    {
    }

    public void NonQueryExecuted(in ProfilerExecutedContext<int> context)
    {
    }

    public void ScalarExecuting(in ProfilerExecutingContext context)
    {
    }

    public void ScalarExecuted(in ProfilerExecutedContext<object?> context)
    {
    }

    public void ReaderExecuting(in ProfilerExecutingContext context)
    {
    }

    public void ReaderExecuted(in ProfilerExecutedContext<DbDataReader> context)
    {
    }

    public void CommandFailed(in ProfilerFailedContext context)
    {
    }

    public void CommandFinally(in ProfilerFinallyContext context)
    {
    }

    public void ReaderFinished(in ProfilerReaderFinishedContext context)
    {
    }
}

internal sealed class Verifier
{
    public bool AllPassed { get; private set; } = true;

    public void Check(string name, bool passed, string detail)
    {
        if (!passed)
        {
            AllPassed = false;
        }

        Console.WriteLine($"  [{(passed ? "PASS" : "FAIL")}] {name}: {detail}");
    }
}
