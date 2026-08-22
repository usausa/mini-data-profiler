namespace MiniDataProfiler;

using System.Data.Common;

public sealed class ProfileDbDataSource : DbDataSource
{
    private readonly IProfileListener listener;

    private readonly DbDataSource dataSource;

    private readonly ProfilerOption? option;

    private readonly bool wrapReader;

    public override string ConnectionString => dataSource.ConnectionString;

    public ProfileDbDataSource(IProfileListener listener, DbDataSource dataSource)
        : this(listener, dataSource, null)
    {
    }

    public ProfileDbDataSource(IProfileListener listener, DbDataSource dataSource, ProfilerOption? option)
    {
        this.listener = RootListener.Wrap(listener);
        this.dataSource = dataSource;
        this.option = option;
        wrapReader = option?.WrapDataReader ?? false;
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
        return new ProfileDbConnection(listener, con, option);
    }

    protected override DbCommand CreateDbCommand(string? commandText = null)
    {
        var cmd = dataSource.CreateCommand(commandText);
        return new ProfileDbDataSourceCommand(listener, cmd, wrapReader);
    }

    protected override DbBatch CreateDbBatch()
    {
        var batch = dataSource.CreateBatch();
        return new ProfileDbDataSourceBatch(listener, batch, wrapReader);
    }
}
