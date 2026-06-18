namespace MiniDataProfiler;

using System.Data.Common;

// ------------------------------------------------------------
// RecordingListener
// ------------------------------------------------------------

internal sealed class RecordingListener : IProfileListener
{
    public List<string> Events { get; } = [];

    public TimeSpan LastExecutedDuration { get; private set; }

    public object? LastScalarResult { get; private set; }

    public int LastNonQueryResult { get; private set; }

    public TimeSpan LastFailedDuration { get; private set; }

    public Exception? LastFailedException { get; private set; }

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
}

// ------------------------------------------------------------
// ThrowingListener
// ------------------------------------------------------------

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
    public void BatchNonQueryExecuting(in BatchProfilerExecutingContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchNonQueryExecuted(in BatchProfilerExecutedContext<int> context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchReaderExecuting(in BatchProfilerExecutingContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchReaderExecuted(in BatchProfilerExecutedContext<DbDataReader> context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchFailed(in BatchProfilerFailedContext context) => throw new InvalidOperationException("ThrowingListener");
    public void BatchFinally(in BatchProfilerFinallyContext context) => throw new InvalidOperationException("ThrowingListener");
}

// ------------------------------------------------------------
// FakeDbDataSource
// ------------------------------------------------------------

internal sealed class FakeDbDataSource : DbDataSource
{
    private readonly string connectionString;

    public override string ConnectionString => connectionString;

    public FakeDbDataSource(string connectionString)
    {
        this.connectionString = connectionString;
    }

    protected override DbConnection CreateDbConnection()
    {
        var con = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
        return con;
    }
}
