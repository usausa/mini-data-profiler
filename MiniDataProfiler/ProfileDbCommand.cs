namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

internal sealed class ProfileDbCommand : DbCommand
{
    private readonly IProfileListener listener;

    private readonly DbCommand cmd;

    private DbConnection? con;

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

    public ProfileDbCommand(IProfileListener listener, ProfileDbConnection con, DbCommand cmd)
    {
        this.listener = listener;
        this.con = con;
        this.cmd = cmd;

        cmd.Connection = con.InnerConnection;
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
        listener.NonQueryExecuting(new ProfilerExecutingContext(EventType.ExecuteNonQuery, this));
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = cmd.ExecuteNonQuery();
            listener.NonQueryExecuted(new ProfilerExecutedContext<int>(EventType.ExecuteNonQuery, this, result, Stopwatch.GetElapsedTime(start)));
            return result;
        }
        catch (Exception ex)
        {
            listener.CommandFailed(new ProfilerFailedContext(EventType.ExecuteNonQuery, this, ex));
            throw;
        }
        finally
        {
            listener.CommandFinally(new ProfilerFinallyContext(EventType.ExecuteNonQuery, this));
        }
    }

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        listener.NonQueryExecuting(new ProfilerExecutingContext(EventType.ExecuteNonQueryAsync, this));
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            listener.NonQueryExecuted(new ProfilerExecutedContext<int>(EventType.ExecuteNonQueryAsync, this, result, Stopwatch.GetElapsedTime(start)));
            return result;
        }
        catch (Exception ex)
        {
            listener.CommandFailed(new ProfilerFailedContext(EventType.ExecuteNonQueryAsync, this, ex));
            throw;
        }
        finally
        {
            listener.CommandFinally(new ProfilerFinallyContext(EventType.ExecuteNonQueryAsync, this));
        }
    }

    public override object? ExecuteScalar()
    {
        listener.ScalarExecuting(new ProfilerExecutingContext(EventType.ExecuteScalar, this));
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = cmd.ExecuteScalar();
            listener.ScalarExecuted(new ProfilerExecutedContext<object?>(EventType.ExecuteScalar, this, result, Stopwatch.GetElapsedTime(start)));
            return result;
        }
        catch (Exception ex)
        {
            listener.CommandFailed(new ProfilerFailedContext(EventType.ExecuteScalar, this, ex));
            throw;
        }
        finally
        {
            listener.CommandFinally(new ProfilerFinallyContext(EventType.ExecuteScalar, this));
        }
    }

    public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        listener.ScalarExecuting(new ProfilerExecutingContext(EventType.ExecuteScalarAsync, this));
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            listener.ScalarExecuted(new ProfilerExecutedContext<object?>(EventType.ExecuteScalarAsync, this, result, Stopwatch.GetElapsedTime(start)));
            return result;
        }
        catch (Exception ex)
        {
            listener.CommandFailed(new ProfilerFailedContext(EventType.ExecuteScalarAsync, this, ex));
            throw;
        }
        finally
        {
            listener.CommandFinally(new ProfilerFinallyContext(EventType.ExecuteScalarAsync, this));
        }
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        listener.ReaderExecuting(new ProfilerExecutingContext(EventType.ExecuteReader, this));
        var start = Stopwatch.GetTimestamp();
        try
        {
            // TODO wrap reader
            var reader = cmd.ExecuteReader(behavior);
            listener.ReaderExecuted(new ProfilerExecutedContext<DbDataReader>(EventType.ExecuteReader, this, reader, Stopwatch.GetElapsedTime(start)));
            return reader;
        }
        catch (Exception ex)
        {
            listener.CommandFailed(new ProfilerFailedContext(EventType.ExecuteReader, this, ex));
            throw;
        }
        finally
        {
            listener.CommandFinally(new ProfilerFinallyContext(EventType.ExecuteReader, this));
        }
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        listener.ReaderExecuting(new ProfilerExecutingContext(EventType.ExecuteReaderAsync, this));
        var start = Stopwatch.GetTimestamp();
        try
        {
            // TODO wrap reader
            var reader = await cmd.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
            listener.ReaderExecuted(new ProfilerExecutedContext<DbDataReader>(EventType.ExecuteReaderAsync, this, reader, Stopwatch.GetElapsedTime(start)));
            return reader;
        }
        catch (Exception ex)
        {
            listener.CommandFailed(new ProfilerFailedContext(EventType.ExecuteReaderAsync, this, ex));
            throw;
        }
        finally
        {
            listener.CommandFinally(new ProfilerFinallyContext(EventType.ExecuteReaderAsync, this));
        }
    }

    // Operation

    public override void Cancel() => cmd.Cancel();

    public override void Prepare() => cmd.Prepare();

    public override Task PrepareAsync(CancellationToken cancellationToken = default) => cmd.PrepareAsync(cancellationToken);

    protected override DbParameter CreateDbParameter() => cmd.CreateParameter();
}
