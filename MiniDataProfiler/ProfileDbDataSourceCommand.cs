namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

internal sealed class ProfileDbDataSourceCommand : DbCommand
{
    private readonly IProfileListener listener;

    private readonly DbCommand cmd;

    private readonly bool wrapReader;

    // ReSharper disable once ReplaceWithFieldKeyword
    private DbConnection? con;

    // ReSharper disable once ReplaceWithFieldKeyword
    private DbTransaction? tx;

    [AllowNull]
#pragma warning disable CA2100
    public override string CommandText
    {
        get => cmd.CommandText;
        set => cmd.CommandText = value;
    }
#pragma warning restore CA2100

    public override int CommandTimeout
    {
        get => cmd.CommandTimeout;
        set => cmd.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => cmd.CommandType;
        set => cmd.CommandType = value;
    }

    protected override DbConnection? DbConnection
    {
        get => con;
        set
        {
            con = value;
            cmd.Connection = value is ProfileDbConnection wrapped ? wrapped.InnerConnection : value;
        }
    }

    protected override DbParameterCollection DbParameterCollection => cmd.Parameters;

    public override bool DesignTimeVisible
    {
        get => cmd.DesignTimeVisible;
        set => cmd.DesignTimeVisible = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => cmd.UpdatedRowSource;
        set => cmd.UpdatedRowSource = value;
    }

    // ReSharper disable once ConvertToAutoProperty
    protected override DbTransaction? DbTransaction
    {
        get => tx;
        set
        {
            tx = value;
            cmd.Transaction = value is ProfileDbTransaction wrapped ? wrapped.InnerTransaction : value;
        }
    }

    public ProfileDbDataSourceCommand(IProfileListener listener, DbCommand cmd, bool wrapReader)
    {
        this.listener = listener;
        this.cmd = cmd;
        this.wrapReader = wrapReader;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cmd.Dispose();
        }

        base.Dispose(disposing);
    }

    // Execute

    public override int ExecuteNonQuery()
    {
        var executingContext = new ProfilerExecutingContext(EventType.ExecuteNonQuery, this);
        listener.NonQueryExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = cmd.ExecuteNonQuery();
            var executedContext = new ProfilerExecutedContext<int>(EventType.ExecuteNonQuery, this, result, Stopwatch.GetElapsedTime(start));
            listener.NonQueryExecuted(in executedContext);
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new ProfilerFailedContext(EventType.ExecuteNonQuery, this, ex, Stopwatch.GetElapsedTime(start));
            listener.CommandFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new ProfilerFinallyContext(EventType.ExecuteNonQuery, this);
            listener.CommandFinally(in finallyContext);
        }
    }

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        var executingContext = new ProfilerExecutingContext(EventType.ExecuteNonQueryAsync, this);
        listener.NonQueryExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            var executedContext = new ProfilerExecutedContext<int>(EventType.ExecuteNonQueryAsync, this, result, Stopwatch.GetElapsedTime(start));
            listener.NonQueryExecuted(in executedContext);
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new ProfilerFailedContext(EventType.ExecuteNonQueryAsync, this, ex, Stopwatch.GetElapsedTime(start));
            listener.CommandFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new ProfilerFinallyContext(EventType.ExecuteNonQueryAsync, this);
            listener.CommandFinally(in finallyContext);
        }
    }

    public override object? ExecuteScalar()
    {
        var executingContext = new ProfilerExecutingContext(EventType.ExecuteScalar, this);
        listener.ScalarExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = cmd.ExecuteScalar();
            var executedContext = new ProfilerExecutedContext<object?>(EventType.ExecuteScalar, this, result, Stopwatch.GetElapsedTime(start));
            listener.ScalarExecuted(in executedContext);
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new ProfilerFailedContext(EventType.ExecuteScalar, this, ex, Stopwatch.GetElapsedTime(start));
            listener.CommandFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new ProfilerFinallyContext(EventType.ExecuteScalar, this);
            listener.CommandFinally(in finallyContext);
        }
    }

    public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        var executingContext = new ProfilerExecutingContext(EventType.ExecuteScalarAsync, this);
        listener.ScalarExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var executedContext = new ProfilerExecutedContext<object?>(EventType.ExecuteScalarAsync, this, result, Stopwatch.GetElapsedTime(start));
            listener.ScalarExecuted(in executedContext);
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new ProfilerFailedContext(EventType.ExecuteScalarAsync, this, ex, Stopwatch.GetElapsedTime(start));
            listener.CommandFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new ProfilerFinallyContext(EventType.ExecuteScalarAsync, this);
            listener.CommandFinally(in finallyContext);
        }
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        var executingContext = new ProfilerExecutingContext(EventType.ExecuteReader, this);
        listener.ReaderExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        IProfilerReaderState? readerState = null;
        try
        {
            var reader = cmd.ExecuteReader(behavior);
            var executedContext = new ProfilerExecutedContext<DbDataReader>(EventType.ExecuteReader, this, reader, Stopwatch.GetElapsedTime(start));
            listener.ReaderExecuted(in executedContext);
            var result = Wrap(reader, EventType.ExecuteReader, start);
            readerState = result as IProfilerReaderState;
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new ProfilerFailedContext(EventType.ExecuteReader, this, ex, Stopwatch.GetElapsedTime(start));
            listener.CommandFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new ProfilerFinallyContext(EventType.ExecuteReader, this, readerState);
            listener.CommandFinally(in finallyContext);
        }
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        var executingContext = new ProfilerExecutingContext(EventType.ExecuteReaderAsync, this);
        listener.ReaderExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        IProfilerReaderState? readerState = null;
        try
        {
            var reader = await cmd.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
            var executedContext = new ProfilerExecutedContext<DbDataReader>(EventType.ExecuteReaderAsync, this, reader, Stopwatch.GetElapsedTime(start));
            listener.ReaderExecuted(in executedContext);
            var result = Wrap(reader, EventType.ExecuteReaderAsync, start);
            readerState = result as IProfilerReaderState;
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new ProfilerFailedContext(EventType.ExecuteReaderAsync, this, ex, Stopwatch.GetElapsedTime(start));
            listener.CommandFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new ProfilerFinallyContext(EventType.ExecuteReaderAsync, this, readerState);
            listener.CommandFinally(in finallyContext);
        }
    }

    private DbDataReader Wrap(DbDataReader reader, EventType eventType, long start) =>
        wrapReader ? new ProfileCommandDbDataReader(listener, this, reader, eventType, start) : reader;

    // Operation

    public override void Cancel() => cmd.Cancel();

    public override void Prepare() => cmd.Prepare();

    public override Task PrepareAsync(CancellationToken cancellationToken = default) => cmd.PrepareAsync(cancellationToken);

    protected override DbParameter CreateDbParameter() => cmd.CreateParameter();
}
