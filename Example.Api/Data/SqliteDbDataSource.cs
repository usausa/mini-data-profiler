namespace Example.Api.Data;

using System.Data.Common;

using Microsoft.Data.Sqlite;

public sealed class SqliteDbDataSource : DbDataSource
{
    public override string ConnectionString { get; }

    public SqliteDbDataSource(string connectionString)
    {
        ConnectionString = connectionString;
    }

    protected override DbConnection CreateDbConnection() => new SqliteConnection(ConnectionString);
}
