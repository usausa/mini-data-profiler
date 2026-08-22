namespace MiniDataProfiler;

using System.Data;

using Microsoft.Data.Sqlite;

public sealed class DelegationTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener? listener = null)
    {
#pragma warning disable CA2000
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener ?? new RecordingListener(), sqlite);
#pragma warning restore CA2000
        con.Open();
        return con;
    }

    [Fact]
    public void CommandTextGetSetIsTransparent()
    {
        // Arrange
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();

        // Act
        cmd.CommandText = "SELECT 1";

        // Assert
        Assert.Equal("SELECT 1", cmd.CommandText);
    }

    [Fact]
    public void CommandTimeoutGetSetIsTransparent()
    {
        // Arrange
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();

        // Act
        cmd.CommandTimeout = 42;

        // Assert
        Assert.Equal(42, cmd.CommandTimeout);
    }

    [Fact]
    public void CommandTypeGetSetIsTransparent()
    {
        // Arrange
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();

        // Act
        cmd.CommandType = CommandType.Text;

        // Assert
        Assert.Equal(CommandType.Text, cmd.CommandType);
    }

    [Fact]
    public void DesignTimeVisibleGetSetIsTransparent()
    {
        // Arrange
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();

        // Act
        cmd.DesignTimeVisible = false;

        // Assert
        Assert.False(cmd.DesignTimeVisible);
    }

    [Fact]
    public void UpdatedRowSourceGetSetIsTransparent()
    {
        // Arrange
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();

        // Act
        cmd.UpdatedRowSource = UpdateRowSource.None;

        // Assert
        Assert.Equal(UpdateRowSource.None, cmd.UpdatedRowSource);
    }

    [Fact]
    public void ParametersWorkWithParameterizedQuery()
    {
        // Arrange
        using var con = CreateConnection();

        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE items (id INTEGER, name TEXT)";
        create.ExecuteNonQuery();

        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO items VALUES (@id, @name)";
        var p1 = ins.CreateParameter();
        p1.ParameterName = "@id";
        p1.Value = 10;
        ins.Parameters.Add(p1);
        var p2 = ins.CreateParameter();
        p2.ParameterName = "@name";
        p2.Value = "test";
        ins.Parameters.Add(p2);
        ins.ExecuteNonQuery();

        // Act
        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT name FROM items WHERE id = @id";
        var p3 = sel.CreateParameter();
        p3.ParameterName = "@id";
        p3.Value = 10;
        sel.Parameters.Add(p3);
        var result = sel.ExecuteScalar();

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void ConnectionSetToProfileDbConnectionInnerCommandUsesInnerConnection()
    {
        // Arrange
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();

        // Act
        cmd.Connection = con;
        cmd.CommandText = "SELECT 42";
        var result = cmd.ExecuteScalar();

        // Assert
        Assert.Equal(42L, result);
    }

    [Fact]
    public void TransactionSetToProfileDbTransactionIsUnwrapped()
    {
        // Arrange
        using var con = CreateConnection();

        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE txtest (val INTEGER)";
        create.ExecuteNonQuery();

        // Act
        using var tx = (ProfileDbTransaction)con.BeginTransaction();

        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO txtest VALUES (99)";
        ins.Transaction = tx;
        ins.ExecuteNonQuery();

        tx.Commit();

        // Assert
        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT val FROM txtest";
        var result = sel.ExecuteScalar();

        Assert.Equal(99L, result);
    }

    [Fact]
    public void TransactionCommitPersistsData()
    {
        // Arrange
        using var con = CreateConnection();
        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE txdata (val INTEGER)";
        create.ExecuteNonQuery();

        // Act
        using var tx = con.BeginTransaction();
        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO txdata VALUES (7)";
        ins.Transaction = tx;
        ins.ExecuteNonQuery();
        tx.Commit();

        // Assert
        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT COUNT(*) FROM txdata";

        Assert.Equal(1L, sel.ExecuteScalar());
    }

    [Fact]
    public void TransactionRollbackRevertsData()
    {
        // Arrange
        using var con = CreateConnection();
        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE txdata2 (val INTEGER)";
        create.ExecuteNonQuery();

        // Act
        using var tx = con.BeginTransaction();
        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO txdata2 VALUES (7)";
        ins.Transaction = tx;
        ins.ExecuteNonQuery();
        tx.Rollback();

        // Assert
        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT COUNT(*) FROM txdata2";

        Assert.Equal(0L, sel.ExecuteScalar());
    }
}
