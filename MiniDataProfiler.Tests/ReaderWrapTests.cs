namespace MiniDataProfiler;

using System.Data;

using Microsoft.Data.Sqlite;

public sealed class ReaderWrapTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener listener, bool wrapReader)
    {
#pragma warning disable CA2000
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite, new ProfilerOption { WrapDataReader = wrapReader });
#pragma warning restore CA2000
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "CREATE TABLE t (id INTEGER PRIMARY KEY, val TEXT)";
        cmd.ExecuteNonQuery();

        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO t VALUES (1, 'v1'), (2, 'v2'), (3, 'v3')";
        ins.ExecuteNonQuery();

        return con;
    }

    [Fact]
    public void ReaderSyncWrappedFiresReaderFinishedAfterDispose()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener, wrapReader: true);

        listener.Events.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            var count = 0;
            while (reader.Read())
            {
                count++;
            }

            Assert.Equal(3, count);

            // ReaderFinished must not fire until the reader is disposed
            Assert.Equal(
                [nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally)],
                listener.Events);
        }

        // Assert
        Assert.Equal(
            [nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally), nameof(IProfileListener.ReaderFinished)],
            listener.Events);
        Assert.Equal(3, listener.LastRecordsRead);
        Assert.Equal(EventType.ExecuteReader, listener.LastReaderFinishedEventType);
        Assert.True(listener.LastReaderFinishedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ReaderAsyncWrappedFiresReaderFinishedAfterDispose()
    {
        // Arrange
        var listener = new RecordingListener();
        await using var con = CreateConnection(listener, wrapReader: true);

        listener.Events.Clear();

        // Act
        var count = 0;
        await using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
#pragma warning disable CA2007
            await using var reader = await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
#pragma warning restore CA2007
            while (await reader.ReadAsync(TestContext.Current.CancellationToken).ConfigureAwait(true))
            {
                count++;
            }
        }

        // Assert
        Assert.Equal(3, count);
        Assert.Equal(
            [nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally), nameof(IProfileListener.ReaderFinished)],
            listener.Events);
        Assert.Equal(3, listener.LastRecordsRead);
        Assert.Equal(EventType.ExecuteReaderAsync, listener.LastReaderFinishedEventType);
    }

    [Fact]
    public void ReaderNotWrappedDoesNotFireReaderFinished()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener, wrapReader: false);

        listener.Events.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
            }
        }

        // Assert
        Assert.DoesNotContain(nameof(IProfileListener.ReaderFinished), listener.Events);
        Assert.Equal(
            [nameof(IProfileListener.ReaderExecuting), nameof(IProfileListener.ReaderExecuted), nameof(IProfileListener.CommandFinally)],
            listener.Events);
    }

    [Fact]
    public void ReaderWrappedPartialReadCountsOnlyConsumedRows()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener, wrapReader: true);

        listener.Events.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal(1L, reader.GetInt64(0));
            Assert.Equal("v1", reader.GetString(1));
        }

        // Assert
        Assert.Equal(1, listener.LastRecordsRead);
        Assert.Contains(nameof(IProfileListener.ReaderFinished), listener.Events);
    }

    [Fact]
    public void ThrowingListenerReaderWrappedSucceedsAndReadsRows()
    {
        // Arrange
        using var con = CreateConnection(new ThrowingListener(), wrapReader: true);

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
        var count = 0;

        // Act
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                count++;
            }
        }

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void ReaderWrappedCloseConnectionBehaviorClosesConnectionOnDispose()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener, wrapReader: true);

        Assert.Equal(ConnectionState.Open, con.State);

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (reader.Read())
            {
            }
        }

        // Assert
        Assert.Equal(ConnectionState.Closed, con.State);
        Assert.Contains(nameof(IProfileListener.ReaderFinished), listener.Events);
    }

    [Fact]
    public void ReaderWrappedSequentialAccessReadsValues()
    {
        // Arrange
        var listener = new RecordingListener();
        using var con = CreateConnection(listener, wrapReader: true);

        listener.Events.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
            using var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            Assert.True(reader.Read());
            Assert.Equal(1L, reader.GetInt64(0));
            Assert.Equal("v1", reader.GetString(1));
        }

        // Assert
        Assert.Equal(1, listener.LastRecordsRead);
        Assert.Contains(nameof(IProfileListener.ReaderFinished), listener.Events);
    }

    [Fact]
    public void DataSourceReaderWrappedFiresReaderFinished()
    {
        // Arrange
        var listener = new RecordingListener();
        using var fake = new FakeDbDataSource("Data Source=:memory:");
        using var ds = new ProfileDbDataSource(listener, fake, new ProfilerOption { WrapDataReader = true });

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE t (id INTEGER, val TEXT); INSERT INTO t VALUES (1, 'a'), (2, 'b')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
            }
        }

        // Assert
        Assert.Contains(nameof(IProfileListener.ReaderFinished), listener.Events);
        Assert.Equal(2, listener.LastRecordsRead);
    }
}
