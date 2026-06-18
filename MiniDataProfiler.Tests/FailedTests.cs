namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class FailedTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener listener)
    {
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite);
        con.Open();
        return con;
    }

    [Fact]
    public void NonQuerySync_ExceptionPropagatesAndEvents()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INVALID SQL";

        Assert.Throws<SqliteException>(() => cmd.ExecuteNonQuery());
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public async Task NonQueryAsync_ExceptionPropagatesAndEvents()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INVALID SQL";

        await Assert.ThrowsAsync<SqliteException>(() => cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken)).ConfigureAwait(true);
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public void ScalarSync_ExceptionPropagatesAndEvents()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        Assert.Throws<SqliteException>(() => cmd.ExecuteScalar());
        Assert.Equal([nameof(IProfileListener.ScalarExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public void ReaderSync_ExceptionPropagatesAndEvents()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        Assert.Throws<SqliteException>(() => cmd.ExecuteReader());
        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public async Task ReaderAsync_ExceptionPropagatesAndEvents()
    {
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        await Assert.ThrowsAsync<SqliteException>(() => cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken)).ConfigureAwait(true);
        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }
}
