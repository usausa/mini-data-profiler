namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;
using System.Diagnostics;

internal sealed class ProfileDbDataSourceBatch : DbBatch
{
    private readonly IProfileListener listener;

    private readonly DbBatch batch;

    private readonly bool wrapReader;

    // ReSharper disable once ReplaceWithFieldKeyword
    private DbConnection? con;

    // ReSharper disable once ReplaceWithFieldKeyword
    private DbTransaction? tx;

    protected override DbBatchCommandCollection DbBatchCommands => batch.BatchCommands;

    public override int Timeout
    {
        get => batch.Timeout;
        set => batch.Timeout = value;
    }

    protected override DbConnection? DbConnection
    {
        get => con;
        set
        {
            con = value;
            batch.Connection = value is ProfileDbConnection wrapped ? wrapped.InnerConnection : value;
        }
    }

    protected override DbTransaction? DbTransaction
    {
        get => tx;
        set
        {
            tx = value;
            batch.Transaction = value is ProfileDbTransaction wrapped ? wrapped.InnerTransaction : value;
        }
    }

    public ProfileDbDataSourceBatch(IProfileListener listener, DbBatch batch, bool wrapReader)
    {
        this.listener = listener;
        this.batch = batch;
        this.wrapReader = wrapReader;
    }

    public override void Dispose()
    {
        batch.Dispose();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await batch.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }

    // Execute

    public override int ExecuteNonQuery()
    {
        var executingContext = new BatchProfilerExecutingContext(EventType.BatchExecuteNonQuery, this);
        listener.BatchNonQueryExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = batch.ExecuteNonQuery();
            var executedContext = new BatchProfilerExecutedContext<int>(EventType.BatchExecuteNonQuery, this, result, Stopwatch.GetElapsedTime(start));
            listener.BatchNonQueryExecuted(in executedContext);
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new BatchProfilerFailedContext(EventType.BatchExecuteNonQuery, this, ex, Stopwatch.GetElapsedTime(start));
            listener.BatchFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new BatchProfilerFinallyContext(EventType.BatchExecuteNonQuery, this);
            listener.BatchFinally(in finallyContext);
        }
    }

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
    {
        var executingContext = new BatchProfilerExecutingContext(EventType.BatchExecuteNonQueryAsync, this);
        listener.BatchNonQueryExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var result = await batch.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            var executedContext = new BatchProfilerExecutedContext<int>(EventType.BatchExecuteNonQueryAsync, this, result, Stopwatch.GetElapsedTime(start));
            listener.BatchNonQueryExecuted(in executedContext);
            return result;
        }
        catch (Exception ex)
        {
            var failedContext = new BatchProfilerFailedContext(EventType.BatchExecuteNonQueryAsync, this, ex, Stopwatch.GetElapsedTime(start));
            listener.BatchFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new BatchProfilerFinallyContext(EventType.BatchExecuteNonQueryAsync, this);
            listener.BatchFinally(in finallyContext);
        }
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        var executingContext = new BatchProfilerExecutingContext(EventType.BatchExecuteReader, this);
        listener.BatchReaderExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var reader = batch.ExecuteReader(behavior);
            var executedContext = new BatchProfilerExecutedContext<DbDataReader>(EventType.BatchExecuteReader, this, reader, Stopwatch.GetElapsedTime(start));
            listener.BatchReaderExecuted(in executedContext);
            return Wrap(reader, EventType.BatchExecuteReader, start);
        }
        catch (Exception ex)
        {
            var failedContext = new BatchProfilerFailedContext(EventType.BatchExecuteReader, this, ex, Stopwatch.GetElapsedTime(start));
            listener.BatchFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new BatchProfilerFinallyContext(EventType.BatchExecuteReader, this);
            listener.BatchFinally(in finallyContext);
        }
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        var executingContext = new BatchProfilerExecutingContext(EventType.BatchExecuteReaderAsync, this);
        listener.BatchReaderExecuting(in executingContext);
        var start = Stopwatch.GetTimestamp();
        try
        {
            var reader = await batch.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
            var executedContext = new BatchProfilerExecutedContext<DbDataReader>(EventType.BatchExecuteReaderAsync, this, reader, Stopwatch.GetElapsedTime(start));
            listener.BatchReaderExecuted(in executedContext);
            return Wrap(reader, EventType.BatchExecuteReaderAsync, start);
        }
        catch (Exception ex)
        {
            var failedContext = new BatchProfilerFailedContext(EventType.BatchExecuteReaderAsync, this, ex, Stopwatch.GetElapsedTime(start));
            listener.BatchFailed(in failedContext);
            throw;
        }
        finally
        {
            var finallyContext = new BatchProfilerFinallyContext(EventType.BatchExecuteReaderAsync, this);
            listener.BatchFinally(in finallyContext);
        }
    }

    private DbDataReader Wrap(DbDataReader reader, EventType eventType, long start) =>
        wrapReader ? new ProfileBatchDbDataReader(listener, this, reader, eventType, start) : reader;

    // Operation

    public override void Cancel() => batch.Cancel();

    public override void Prepare() => batch.Prepare();

    public override Task PrepareAsync(CancellationToken cancellationToken = default) => batch.PrepareAsync(cancellationToken);

    public override object? ExecuteScalar() => batch.ExecuteScalar();

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default) => batch.ExecuteScalarAsync(cancellationToken);

    protected override DbBatchCommand CreateDbBatchCommand() => batch.CreateBatchCommand();
}
