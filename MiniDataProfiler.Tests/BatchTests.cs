namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class BatchTests
{
    private static bool SqliteCanCreateBatch()
    {
#pragma warning disable CA2000
        using var con = new SqliteConnection("Data Source=:memory:");
        return con.CanCreateBatch;
#pragma warning restore CA2000
    }

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
    public void BatchNonQuerySyncEventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        batch.BatchCommands.Add(batchCmd);

        var result = batch.ExecuteNonQuery();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchNonQueryExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task BatchNonQueryAsyncEventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        await using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "INSERT INTO t VALUES (3, 'async')";
        batch.BatchCommands.Add(batchCmd);

        var result = await batch.ExecuteNonQueryAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchNonQueryExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void BatchReaderSyncEventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        batch.BatchCommands.Add(batchCmd);

        using var reader = batch.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
        Assert.Equal([nameof(IProfileListener.BatchReaderExecuting), nameof(IProfileListener.BatchReaderExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task BatchReaderAsyncEventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        await using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        batch.BatchCommands.Add(batchCmd);

#pragma warning disable CA2007
        await using var reader = await batch.ExecuteReaderAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
#pragma warning restore CA2007

        // Assert
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken).ConfigureAwait(true));
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
        Assert.Equal([nameof(IProfileListener.BatchReaderExecuting), nameof(IProfileListener.BatchReaderExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void BatchReaderWrappedFiresBatchReaderFinishedAfterDispose()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        // Arrange
        var listener = new RecordingListener();
#pragma warning disable CA2000
        var sqlite = new SqliteConnection("Data Source=:memory:");
#pragma warning restore CA2000
        using var con = new ProfileDbConnection(listener, sqlite, new ProfilerOption { WrapDataReader = true });
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE t (id INTEGER PRIMARY KEY, val TEXT); INSERT INTO t VALUES (1, 'a'), (2, 'b'), (3, 'c')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        // Act
        using (var batch = con.CreateBatch())
        {
            var batchCmd = batch.CreateBatchCommand();
            batchCmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            batch.BatchCommands.Add(batchCmd);

            using var reader = batch.ExecuteReader();
            var count = 0;
            while (reader.Read())
            {
                count++;
            }

            Assert.Equal(3, count);

            // BatchReaderFinished must not fire until the reader is disposed
            Assert.DoesNotContain(nameof(IProfileListener.BatchReaderFinished), listener.Events);
        }

        // Assert
        Assert.Contains(nameof(IProfileListener.BatchReaderFinished), listener.Events);
        Assert.Equal(3, listener.LastRecordsRead);
        Assert.Equal(EventType.BatchExecuteReader, listener.LastReaderFinishedEventType);
    }

    [Fact]
    public void BatchNonQueryFailedEventsAndDuration()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();

        // Act
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "INVALID SQL";
        batch.BatchCommands.Add(batchCmd);

        // Assert
        Assert.Throws<SqliteException>(() => batch.ExecuteNonQuery());
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchFailed), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    //--------------------------------------------------------------------------------
    // Fake batch
    //--------------------------------------------------------------------------------

    [Fact]
    public void FakeBatchNonQuerySyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
#pragma warning disable CA2000
        var innerCon = new FakeInnerBatchDbConnection(failBatch: false);
#pragma warning restore CA2000
        using var profileCon = new ProfileDbConnection(listener, innerCon);
        profileCon.Open();

        listener.Events.Clear();

        // Act
        using var batch = profileCon.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT 1";
        batch.BatchCommands.Add(batchCmd);

        var result = batch.ExecuteNonQuery();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchNonQueryExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void FakeBatchFailedEventsAndDuration()
    {
        // Arrange
        var listener = new RecordingListener();
#pragma warning disable CA2000
        var innerCon = new FakeInnerBatchDbConnection(failBatch: true);
#pragma warning restore CA2000
        using var profileCon = new ProfileDbConnection(listener, innerCon);
        profileCon.Open();

        listener.Events.Clear();

        // Act
        using var batch = profileCon.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT 1";
        batch.BatchCommands.Add(batchCmd);

        // Assert
        Assert.Throws<InvalidOperationException>(() => batch.ExecuteNonQuery());
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchFailed), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<InvalidOperationException>(listener.LastFailedException);
    }
}
