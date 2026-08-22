namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class DataSourceTests
{
    private static ProfileDbDataSource CreateDataSource(IProfileListener listener)
    {
#pragma warning disable CA2000
        var fake = new FakeDbDataSource("Data Source=:memory:");
#pragma warning restore CA2000
        return new ProfileDbDataSource(listener, fake);
    }

    //--------------------------------------------------------------------------------
    // Connection
    //--------------------------------------------------------------------------------

    [Fact]
    public void CreateConnectionNonQuerySyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE t (id INTEGER, val TEXT); INSERT INTO t VALUES (1, 'hello')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        var result = cmd.ExecuteNonQuery();

        // Assert
        Assert.Equal(1, result);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuted), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), listener.Events);
    }

    //--------------------------------------------------------------------------------
    // Command
    //--------------------------------------------------------------------------------

    [Fact]
    public void DataSourceCommandNonQuerySyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var innerCon = new SqliteConnection("Data Source=:memory:");
        innerCon.Open();

        using var setup = innerCon.CreateCommand();
        setup.CommandText = "CREATE TABLE t2 (id INTEGER, val TEXT)";
        setup.ExecuteNonQuery();

        using var profileCon = ds.CreateConnection();
        profileCon.Open();

        using var createTable = profileCon.CreateCommand();
        createTable.CommandText = "CREATE TABLE dstest (id INTEGER, val TEXT)";
        createTable.ExecuteNonQuery();

        listener.Events.Clear();

        // Act
        using var cmd = profileCon.CreateCommand();
        cmd.CommandText = "INSERT INTO dstest VALUES (1, 'data')";
        var result = cmd.ExecuteNonQuery();

        // Assert
        Assert.Equal(1, result);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuted), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), listener.Events);
    }

    [Fact]
    public void DataSourceCommandScalarSyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE sc (id INTEGER, val TEXT); INSERT INTO sc VALUES (1, 'hello')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM sc WHERE id = 1";
        var result = cmd.ExecuteScalar();

        // Assert
        Assert.Equal("hello", result);
        Assert.Contains(nameof(IProfileListener.ScalarExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.ScalarExecuted), listener.Events);
    }

    [Fact]
    public void DataSourceCommandReaderSyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE rd (id INTEGER, val TEXT); INSERT INTO rd VALUES (1, 'hello')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM rd WHERE id = 1";
        using var reader = cmd.ExecuteReader();

        // Assert
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));
        Assert.Contains(nameof(IProfileListener.ReaderExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.ReaderExecuted), listener.Events);
    }

    [Fact]
    public void DataSourceCommandFailedEventsAndDuration()
    {
        // Arrange
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        listener.Events.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        // Act & Assert
        Assert.Throws<SqliteException>(cmd.ExecuteScalar);

        Assert.Contains(nameof(IProfileListener.ScalarExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFailed), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public async Task DataSourceCommandScalarAsyncEventsAndResult()
    {
        // Arrange
        var listener = new RecordingListener();
        await using var ds = CreateDataSource(listener);

        await using var con = ds.CreateConnection();
        await con.OpenAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        await using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE sca (id INTEGER, val TEXT); INSERT INTO sca VALUES (1, 'async')";
        await setup.ExecuteNonQueryAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        listener.Events.Clear();

        // Act
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM sca WHERE id = 1";
        var result = await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        Assert.Equal("async", result);
        Assert.Contains(nameof(IProfileListener.ScalarExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.ScalarExecuted), listener.Events);
    }
}
