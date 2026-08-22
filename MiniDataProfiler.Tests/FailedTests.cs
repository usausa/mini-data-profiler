namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class FailedTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener listener)
    {
#pragma warning disable CA2000
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite);
#pragma warning restore CA2000
        con.Open();
        return con;
    }

    [Fact]
    public void NonQuerySyncExceptionPropagatesAndEvents()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INVALID SQL";

        // Act & Assert
        Assert.Throws<SqliteException>(() => cmd.ExecuteNonQuery());
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public async Task NonQueryAsyncExceptionPropagatesAndEvents()
    {
        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener);

        await using var cmd = con.CreateCommand();
        cmd.CommandText = "INVALID SQL";

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken)).ConfigureAwait(true);
        Assert.Equal([nameof(IProfileListener.NonQueryExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public void ScalarSyncExceptionPropagatesAndEvents()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        // Act & Assert
        Assert.Throws<SqliteException>(cmd.ExecuteScalar);
        Assert.Equal([nameof(IProfileListener.ScalarExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public void ReaderSyncExceptionPropagatesAndEvents()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        // Act & Assert
        Assert.Throws<SqliteException>(cmd.ExecuteReader);
        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public async Task ReaderAsyncExceptionPropagatesAndEvents()
    {
        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener);

        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken)).ConfigureAwait(true);
        Assert.Equal([nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.CommandFailed), nameof(IProfileListener.CommandFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }
}
