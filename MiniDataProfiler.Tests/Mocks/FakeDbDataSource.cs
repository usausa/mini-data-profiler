namespace MiniDataProfiler.Mocks;

using System.Data.Common;

internal sealed class FakeDbDataSource : DbDataSource
{
    public override string ConnectionString { get; }

    public FakeDbDataSource(string connectionString)
    {
        ConnectionString = connectionString;
    }

    protected override DbConnection CreateDbConnection()
    {
        var con = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        return con;
    }
}
