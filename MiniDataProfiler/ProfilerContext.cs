namespace MiniDataProfiler;

using System.Data.Common;

public readonly struct ProfilerExecutingContext : IEquatable<ProfilerExecutingContext>
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public ProfilerExecutingContext(EventType eventType, DbCommand command)
    {
        EventType = eventType;
        Command = command;
    }

    public bool Equals(ProfilerExecutingContext other) => EventType == other.EventType && Command.Equals(other.Command);

    public override bool Equals(object? obj) => obj is ProfilerExecutingContext other && Equals(other);

    public override int GetHashCode() => HashCode.Combine((int)EventType, Command);

    public static bool operator ==(ProfilerExecutingContext left, ProfilerExecutingContext right) => left.Equals(right);

    public static bool operator !=(ProfilerExecutingContext left, ProfilerExecutingContext right) => !left.Equals(right);
}

public readonly struct ProfilerExecutedContext<T> : IEquatable<ProfilerExecutedContext<T>>
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

    public bool Equals(ProfilerExecutedContext<T> other) => EventType == other.EventType && Command.Equals(other.Command) && EqualityComparer<T>.Default.Equals(Result, other.Result) && Duration.Equals(other.Duration);

    public override bool Equals(object? obj) => obj is ProfilerExecutedContext<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine((int)EventType, Command, Result, Duration);

    public static bool operator ==(ProfilerExecutedContext<T> left, ProfilerExecutedContext<T> right) => left.Equals(right);

    public static bool operator !=(ProfilerExecutedContext<T> left, ProfilerExecutedContext<T> right) => !left.Equals(right);
}

public readonly struct ProfilerFailedContext : IEquatable<ProfilerFailedContext>
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

    public bool Equals(ProfilerFailedContext other) => EventType == other.EventType && Command.Equals(other.Command) && Exception.Equals(other.Exception);

    public override bool Equals(object? obj) => obj is ProfilerFailedContext other && Equals(other);

    public override int GetHashCode() => HashCode.Combine((int)EventType, Command, Exception);

    public static bool operator ==(ProfilerFailedContext left, ProfilerFailedContext right) => left.Equals(right);

    public static bool operator !=(ProfilerFailedContext left, ProfilerFailedContext right) => !left.Equals(right);
}

public readonly struct ProfilerFinallyContext : IEquatable<ProfilerFinallyContext>
{
    public EventType EventType { get; }

    public DbCommand Command { get; }

    public ProfilerFinallyContext(EventType eventType, DbCommand command)
    {
        EventType = eventType;
        Command = command;
    }

    public bool Equals(ProfilerFinallyContext other) => EventType == other.EventType && Command.Equals(other.Command);

    public override bool Equals(object? obj) => obj is ProfilerFinallyContext other && Equals(other);

    public override int GetHashCode() => HashCode.Combine((int)EventType, Command);

    public static bool operator ==(ProfilerFinallyContext left, ProfilerFinallyContext right) => left.Equals(right);

    public static bool operator !=(ProfilerFinallyContext left, ProfilerFinallyContext right) => !left.Equals(right);
}
