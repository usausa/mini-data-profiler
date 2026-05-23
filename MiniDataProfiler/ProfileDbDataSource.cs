namespace MiniDataProfiler;

using System.Data.Common;

public sealed class ProfileDbDataSource : DbDataSource
{
    private readonly IProfileListener listener;

    private readonly DbDataSource dataSource;

    public override string ConnectionString => dataSource.ConnectionString;

    public ProfileDbDataSource(IProfileListener listener, DbDataSource dataSource)
    {
        this.listener = listener;
        this.dataSource = dataSource;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            dataSource.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override DbConnection CreateDbConnection()
    {
        var con = dataSource.CreateConnection();
        return new ProfileDbConnection(listener, con);
    }

    protected override DbCommand CreateDbCommand(string? commandText = null)
    {
        var cmd = dataSource.CreateCommand(commandText);
        return new ProfileDbDataSourceCommand(listener, cmd);
    }

    protected override DbBatch CreateDbBatch()
    {
        var batch = dataSource.CreateBatch();
        return new ProfileDbDataSourceBatch(listener, batch);
    }
}
