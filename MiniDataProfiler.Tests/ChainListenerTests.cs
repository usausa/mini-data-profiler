namespace MiniDataProfiler;

using Microsoft.Data.Sqlite;

public sealed class ChainListenerTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener listener)
    {
#pragma warning disable CA2000
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite);
#pragma warning restore CA2000
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
    public void ChainListenerAllEventsDeliveredToBothListeners()
    {
        // Arrange
        var recorder1 = new RecordingListener();
        var recorder2 = new RecordingListener();
        var chain = new ChainListener(recorder1, recorder2);

        using var con = CreateConnection(chain);

        recorder1.Events.Clear();
        recorder2.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        // Assert
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
    public void ChainListenerOneThrowsOtherReceivesEventsAndDbSucceeds()
    {
        // Arrange
        var recorder = new RecordingListener();
        var thrower = new ThrowingListener();
        var chain = new ChainListener(thrower, recorder);

        using var con = CreateConnection(chain);

        recorder.Events.Clear();

        // Act
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT val FROM t WHERE id = 1";
        var result = cmd.ExecuteScalar();

        // Assert
        Assert.Equal("hello", result);

        Assert.Equal(3, recorder.Events.Count);
        Assert.Contains(nameof(IProfileListener.ScalarExecuting), recorder.Events);
        Assert.Contains(nameof(IProfileListener.ScalarExecuted), recorder.Events);
        Assert.Contains(nameof(IProfileListener.CommandFinally), recorder.Events);
    }
}
