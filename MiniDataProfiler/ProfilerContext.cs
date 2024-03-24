namespace MiniDataProfiler;

using System.Data.Common;

public readonly ref struct ProfilerExecutingContext
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public ProfilerExecutingContext(EventType eventType, DbCommand command)
    {
        EventType = eventType;
        Command = command;
    }
}

public readonly ref struct ProfilerExecutedContext<T>
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

public readonly ref struct ProfilerFailedContext
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public Exception Exception { get; }

    public ProfilerFailedContext(EventType eventType, DbCommand command, Exception exception)
    {
        EventType = eventType;
        Command = command;
        Exception = exception;
    }
}

public readonly ref struct ProfilerFinallyContext
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public ProfilerFinallyContext(EventType eventType, DbCommand command)
    {
        EventType = eventType;
        Command = command;
    }
}
