namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class DataSourceTests
{
    private static ProfileDbDataSource CreateDataSource(IProfileListener listener)
    {
        var fake = new FakeDbDataSource("Data Source=:memory:");
        return new ProfileDbDataSource(listener, fake);
    }

    // CreateConnection() path

    [Fact]
    public void CreateConnection_NonQuerySync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE t (id INTEGER, val TEXT); INSERT INTO t VALUES (1, 'hello')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        var result = cmd.ExecuteNonQuery();

        Assert.Equal(1, result);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuted), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), listener.Events);
    }

    // CreateCommand() path - ProfileDbDataSourceCommand

    [Fact]
    public void DataSourceCommand_NonQuerySync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        // The DataSource-level command needs a connection to execute against.
        // ProfileDbDataSource.CreateCommand() returns a ProfileDbDataSourceCommand
        // that wraps the inner DataSource's command. The inner command from FakeDbDataSource
        // has no connection attached, so we open a connection and set it explicitly.
        using var innerCon = new SqliteConnection("Data Source=:memory:");
        innerCon.Open();

        using var setup = innerCon.CreateCommand();
        setup.CommandText = "CREATE TABLE t2 (id INTEGER, val TEXT)";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        // We need to manually create a ProfileDbDataSourceCommand wrapping an inner command
        // that has the connection set. Create via CreateConnection + CreateCommand pattern.
        using var profilCon = ds.CreateConnection();
        profilCon.Open();

        using var createTable = profilCon.CreateCommand();
        createTable.CommandText = "CREATE TABLE dstest (id INTEGER, val TEXT)";
        createTable.ExecuteNonQuery();

        listener.Events.Clear();

        using var cmd = profilCon.CreateCommand();
        cmd.CommandText = "INSERT INTO dstest VALUES (1, 'data')";
        var result = cmd.ExecuteNonQuery();

        Assert.Equal(1, result);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.NonQueryExecuted), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), listener.Events);
    }

    [Fact]
    public void DataSourceCommand_ScalarSync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE sc (id INTEGER, val TEXT); INSERT INTO sc VALUES (1, 'hello')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM sc WHERE id = 1";
        var result = cmd.ExecuteScalar();

        Assert.Equal("hello", result);
        Assert.Contains(nameof(IProfileListener.ScalarExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.ScalarExecuted), listener.Events);
    }

    [Fact]
    public void DataSourceCommand_ReaderSync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE rd (id INTEGER, val TEXT); INSERT INTO rd VALUES (1, 'hello')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM rd WHERE id = 1";
        using var reader = cmd.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));

        Assert.Contains(nameof(IProfileListener.ReaderExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.ReaderExecuted), listener.Events);
    }

    [Fact]
    public void DataSourceCommand_Failed_EventsAndDuration()
    {
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        listener.Events.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT * FROM nonexistent_table";

        Assert.Throws<SqliteException>(() => cmd.ExecuteScalar());

        Assert.Contains(nameof(IProfileListener.ScalarExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFailed), listener.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    [Fact]
    public async Task DataSourceCommand_ScalarAsync_EventsAndResult()
    {
        var listener = new RecordingListener();
        using var ds = CreateDataSource(listener);

        using var con = ds.CreateConnection();
        con.Open();

        using var setup = con.CreateCommand();
        setup.CommandText = "CREATE TABLE sca (id INTEGER, val TEXT); INSERT INTO sca VALUES (1, 'async')";
        setup.ExecuteNonQuery();

        listener.Events.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM sca WHERE id = 1";
        var result = await cmd.ExecuteScalarAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal("async", result);
        Assert.Contains(nameof(IProfileListener.ScalarExecuting), listener.Events);
        Assert.Contains(nameof(IProfileListener.ScalarExecuted), listener.Events);
    }
}
