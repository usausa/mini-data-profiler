namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

internal sealed class ProfileDbCommand : DbCommand
{
    private readonly IDataProfiler profiler;

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
            cmd.Connection = con is ProfileDbConnection wrapped ? wrapped.InnerConnection : value;
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
        set => tx = value;
    }

    public ProfileDbCommand(IDataProfiler profiler, ProfileDbConnection con, DbCommand cmd)
    {
        this.profiler = profiler;
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
        profiler.OnExecuteStart(this);
        var start = ProfileHelper.GetTimeStamp();
        try
        {
            return cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            profiler.OnError(this, e);
            throw;
        }
        finally
        {
            profiler.OnExecuteFinally(this, ProfileHelper.GetElapsed(start));
        }
    }

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        profiler.OnExecuteStart(this);
        var start = ProfileHelper.GetTimeStamp();
        try
        {
            return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            profiler.OnError(this, e);
            throw;
        }
        finally
        {
            profiler.OnExecuteFinally(this, ProfileHelper.GetElapsed(start));
        }
    }

    public override object? ExecuteScalar()
    {
        profiler.OnExecuteStart(this);
        var start = ProfileHelper.GetTimeStamp();
        try
        {
            return cmd.ExecuteScalar();
        }
        catch (Exception e)
        {
            profiler.OnError(this, e);
            throw;
        }
        finally
        {
            profiler.OnExecuteFinally(this, ProfileHelper.GetElapsed(start));
        }
    }

    public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        profiler.OnExecuteStart(this);
        var start = ProfileHelper.GetTimeStamp();
        try
        {
            return await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            profiler.OnError(this, e);
            throw;
        }
        finally
        {
            profiler.OnExecuteFinally(this, ProfileHelper.GetElapsed(start));
        }
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        profiler.OnExecuteStart(this);
        var start = ProfileHelper.GetTimeStamp();
        try
        {
            // TODO reader
            return cmd.ExecuteReader(behavior);
        }
        catch (Exception e)
        {
            profiler.OnError(this, e);
            throw;
        }
        finally
        {
            profiler.OnExecuteFinally(this, ProfileHelper.GetElapsed(start));
        }
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        profiler.OnExecuteStart(this);
        var start = ProfileHelper.GetTimeStamp();
        try
        {
            // TODO reader
            return await cmd.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            profiler.OnError(this, e);
            throw;
        }
        finally
        {
            profiler.OnExecuteFinally(this, ProfileHelper.GetElapsed(start));
        }
    }

    // Operation

    public override void Cancel() => cmd.Cancel();

    public override void Prepare() => cmd.Prepare();

    public override Task PrepareAsync(CancellationToken cancellationToken = default) => cmd.PrepareAsync(cancellationToken);

    protected override DbParameter CreateDbParameter() => cmd.CreateParameter();
}
