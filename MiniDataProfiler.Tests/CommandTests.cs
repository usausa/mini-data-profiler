namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class CommandTests
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
    public void NonQuerySync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        var result = cmd.ExecuteNonQuery();

        Assert.Equal(1, result);
        Assert.Equal(1, listener.LastNonQueryResult);
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.NonQueryExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task NonQueryAsync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (3, 'async')";
        var result = await cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal(1, result);
        Assert.Equal(1, listener.LastNonQueryResult);
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.NonQueryExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void ScalarSync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        Assert.Equal("hello", result);
        Assert.Equal("hello", listener.LastScalarResult);
        Assert.Equal([nameof(IProfileListener.ScalarExecuting), nameof(IProfileListener.ScalarExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ScalarAsync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal("hello", result);
        Assert.Equal("hello", listener.LastScalarResult);
        Assert.Equal([nameof(IProfileListener.ScalarExecuting), nameof(IProfileListener.ScalarExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void ReaderSync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        using var reader = cmd.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));

        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ReaderAsync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        using var reader = await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken).ConfigureAwait(true));
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));

        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }
}
