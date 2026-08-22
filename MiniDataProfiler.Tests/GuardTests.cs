namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class GuardTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener listener)
    {
#pragma warning disable CA2000
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite);
#pragma warning restore CA2000
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
    public void ThrowingListenerNonQuerySyncSucceedsAndReturnsCorrectResult()
    {
        // Arrange
        using var con = CreateConnection(new ThrowingListener());

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        var result = cmd.ExecuteNonQuery();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ThrowingListenerNonQueryAsyncSucceedsAndReturnsCorrectResult()
    {
        // Arrange
        await using var con = CreateConnection(new ThrowingListener());

        // Act
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (3, 'async')";
        var result = await cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void ThrowingListenerScalarSyncSucceedsAndReturnsCorrectResult()
    {
        // Arrange
        using var con = CreateConnection(new ThrowingListener());

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ThrowingListenerReaderSyncSucceedsAndReturnsRows()
    {
        // Arrange
        using var con = CreateConnection(new ThrowingListener());

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        using var reader = cmd.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
    }

    [Fact]
    public async Task ThrowingListenerReaderAsyncSucceedsAndReturnsRows()
    {
        // Arrange
        await using var con = CreateConnection(new ThrowingListener());

        // Act
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
#pragma warning disable CA2007
        await using var reader = await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
#pragma warning restore CA2007

        // Assert
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken).ConfigureAwait(true));
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
    }

    [Fact]
    public void ThrowingListenerFailedSqlThrowsSqliteExceptionNotInvalidOperationException()
    {
        // Arrange
        using var con = CreateConnection(new ThrowingListener());

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INVALID SQL";

        // Act & Assert
        var ex = Assert.Throws<SqliteException>(() => cmd.ExecuteNonQuery());
        Assert.IsType<SqliteException>(ex);
    }
}
