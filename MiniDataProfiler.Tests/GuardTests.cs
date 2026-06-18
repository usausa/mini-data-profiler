namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class GuardTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener listener)
    {
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "CREATE TABLE t (id INTEGER PRIMARY KEY, val TEXT)";
        cmd.ExecuteNonQuery();

        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO t VALUES (1, 'hello')";
        ins.ExecuteNonQuery();

        return con;
    }

    [Fact]
    public void ThrowingListener_NonQuerySync_SucceedsAndReturnsCorrectResult()
    {
        using var con = CreateConnection(new ThrowingListener());

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        var result = cmd.ExecuteNonQuery();

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ThrowingListener_NonQueryAsync_SucceedsAndReturnsCorrectResult()
    {
        using var con = CreateConnection(new ThrowingListener());

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (3, 'async')";
        var result = await cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal(1, result);
    }

    [Fact]
    public void ThrowingListener_ScalarSync_SucceedsAndReturnsCorrectResult()
    {
        using var con = CreateConnection(new ThrowingListener());

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        Assert.Equal("hello", result);
    }

    [Fact]
    public void ThrowingListener_ReaderSync_SucceedsAndReturnsRows()
    {
        using var con = CreateConnection(new ThrowingListener());

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        using var reader = cmd.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
    }

    [Fact]
    public async Task ThrowingListener_ReaderAsync_SucceedsAndReturnsRows()
    {
        using var con = CreateConnection(new ThrowingListener());

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        using var reader = await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken).ConfigureAwait(true));
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
    }

    [Fact]
    public void ThrowingListener_FailedSql_ThrowsSqliteExceptionNotInvalidOperationException()
    {
        using var con = CreateConnection(new ThrowingListener());

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INVALID SQL";

        var ex = Assert.Throws<SqliteException>(() => cmd.ExecuteNonQuery());
        Assert.IsType<SqliteException>(ex);
    }
}
