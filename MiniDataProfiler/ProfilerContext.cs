namespace MiniDataProfiler;

using System.Data.Common;

#pragma warning disable CA1815
public readonly struct ProfilerExecutingContext
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public ProfilerExecutingContext(EventType eventType, DbCommand command)
    {
        EventType = eventType;
        Command = command;
    }
}

public readonly struct ProfilerExecutedContext<T>
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public T Result { get; }

    public TimeSpan Duration { get; }

    public ProfilerExecutedContext(EventType eventType, DbCommand command, T result, TimeSpan duration)
    {
        EventType = eventType;
        Command = command;
        Result = result;
        Duration = duration;
    }
}

public readonly struct ProfilerFailedContext
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public Exception Exception { get; }

    public TimeSpan Duration { get; }

    public ProfilerFailedContext(EventType eventType, DbCommand command, Exception exception, TimeSpan duration)
    {
        EventType = eventType;
        Command = command;
        Exception = exception;
        Duration = duration;
    }
}

public readonly struct ProfilerFinallyContext
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public ProfilerFinallyContext(EventType eventType, DbCommand command)
    {
        EventType = eventType;
        Command = command;
    }
}

public readonly struct ProfilerReaderFinishedContext
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public int RecordsRead { get; }

    public TimeSpan Duration { get; }

    public ProfilerReaderFinishedContext(EventType eventType, DbCommand command, int recordsRead, TimeSpan duration)
    {
        EventType = eventType;
        Command = command;
        RecordsRead = recordsRead;
        Duration = duration;
    }
}

public readonly struct BatchProfilerExecutingContext
{
    public EventType EventType { get; }

    public DbBatch Batch { get; }

    public BatchProfilerExecutingContext(EventType eventType, DbBatch batch)
    {
        EventType = eventType;
        Batch = batch;
    }
}

public readonly struct BatchProfilerExecutedContext<T>
{
    public EventType EventType { get; }

    public DbBatch Batch { get; }

    public T Result { get; }

    public TimeSpan Duration { get; }

    public BatchProfilerExecutedContext(EventType eventType, DbBatch batch, T result, TimeSpan duration)
    {
        EventType = eventType;
        Batch = batch;
        Result = result;
        Duration = duration;
    }
}

public readonly struct BatchProfilerFailedContext
{
    public EventType EventType { get; }

    public DbBatch Batch { get; }

    public Exception Exception { get; }

    public TimeSpan Duration { get; }

    public BatchProfilerFailedContext(EventType eventType, DbBatch batch, Exception exception, TimeSpan duration)
    {
        EventType = eventType;
        Batch = batch;
        Exception = exception;
        Duration = duration;
    }
}

public readonly struct BatchProfilerFinallyContext
{
    public EventType EventType { get; }

    public DbBatch Batch { get; }

    public BatchProfilerFinallyContext(EventType eventType, DbBatch batch)
    {
        EventType = eventType;
        Batch = batch;
    }
}

public readonly struct BatchProfilerReaderFinishedContext
{
    public EventType EventType { get; }

    public DbBatch Batch { get; }

    public int RecordsRead { get; }

    public TimeSpan Duration { get; }

    public BatchProfilerReaderFinishedContext(EventType eventType, DbBatch batch, int recordsRead, TimeSpan duration)
    {
        EventType = eventType;
        Batch = batch;
        RecordsRead = recordsRead;
        Duration = duration;
    }
}
#pragma warning restore CA1815
