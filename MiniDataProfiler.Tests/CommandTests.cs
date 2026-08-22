namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class CommandTests
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
    public void NonQuerySyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        var result = cmd.ExecuteNonQuery();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, listener.LastNonQueryResult);
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.NonQueryExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task NonQueryAsyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (3, 'async')";
        var result = await cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, listener.LastNonQueryResult);
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.NonQueryExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void ScalarSyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        // Assert
        Assert.Equal("hello", result);
        Assert.Equal("hello", listener.LastScalarResult);
        Assert.Equal([nameof(IProfileListener.ScalarExecuting), nameof(IProfileListener.ScalarExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ScalarAsyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        Assert.Equal("hello", result);
        Assert.Equal("hello", listener.LastScalarResult);
        Assert.Equal([nameof(IProfileListener.ScalarExecuting), nameof(IProfileListener.ScalarExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void ReaderSyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        using var reader = cmd.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ReaderAsyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener);

        listener.Events.Clear();

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
        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }
}
