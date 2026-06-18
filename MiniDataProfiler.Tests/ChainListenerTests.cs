namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class ChainListenerTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener listener)
    {
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "CREATE TABLE t (id INTEGER, val TEXT)";
        cmd.ExecuteNonQuery();

        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO t VALUES (1, 'hello')";
        ins.ExecuteNonQuery();

        return con;
    }

    [Fact]
    public void ChainListener_AllEventsDeliveredToBothListeners()
    {
        var recorder1 = new RecordingListener();
        var recorder2 = new RecordingListener();
        var chain = new ChainListener(recorder1, recorder2);

        using var con = CreateConnection(chain);

        recorder1.Events.Clear();
        recorder2.Events.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        Assert.Equal("hello", result);

        var expected = new[]
        {
            nameof(IProfileListener.ScalarExecuting),
            nameof(IProfileListener.ScalarExecuted),
            nameof(IProfileListener.CommandFinally)
        };

        Assert.Equal(expected, recorder1.Events);
        Assert.Equal(expected, recorder2.Events);
    }

    [Fact]
    public void ChainListener_OneThrows_OtherReceivesEventsAndDbSucceeds()
    {
        var recorder = new RecordingListener();
        var thrower = new ThrowingListener();
        var chain = new ChainListener(thrower, recorder);

        using var con = CreateConnection(chain);

        recorder.Events.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        Assert.Equal("hello", result);

        // recorder should still receive all events
        Assert.Equal(3, recorder.Events.Count);
        Assert.Contains(nameof(IProfileListener.ScalarExecuting), recorder.Events);
        Assert.Contains(nameof(IProfileListener.ScalarExecuted), recorder.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), recorder.Events);
    }
}
