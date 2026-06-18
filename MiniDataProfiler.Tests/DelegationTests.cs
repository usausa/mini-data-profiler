namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;

using Microsoft.Data.Sqlite;

public sealed class DelegationTests
{
    private static ProfileDbConnection CreateConnection(IProfileListener? listener = null)
    {
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener ?? new RecordingListener(), sqlite);
        con.Open();
        return con;
    }

    [Fact]
    public void CommandText_GetSet_IsTransparent()
    {
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT 1";
        Assert.Equal("SELECT 1", cmd.CommandText);
    }

    [Fact]
    public void CommandTimeout_GetSet_IsTransparent()
    {
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandTimeout = 42;
        Assert.Equal(42, cmd.CommandTimeout);
    }

    [Fact]
    public void CommandType_GetSet_IsTransparent()
    {
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandType = CommandType.Text;
        Assert.Equal(CommandType.Text, cmd.CommandType);
    }

    [Fact]
    public void DesignTimeVisible_GetSet_IsTransparent()
    {
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();
        cmd.DesignTimeVisible = false;
        Assert.False(cmd.DesignTimeVisible);
    }

    [Fact]
    public void UpdatedRowSource_GetSet_IsTransparent()
    {
        using var con = CreateConnection();
        using var cmd = con.CreateCommand();
        cmd.UpdatedRowSource = UpdateRowSource.None;
        Assert.Equal(UpdateRowSource.None, cmd.UpdatedRowSource);
    }

    [Fact]
    public void Parameters_WorkWithParameterizedQuery()
    {
        using var con = CreateConnection();

        // create table
        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE items (id INTEGER, name TEXT)";
        create.ExecuteNonQuery();

        // insert with parameter
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

        // query with parameter
        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT name FROM items WHERE id = @id";
        var p3 = sel.CreateParameter();
        p3.ParameterName = "@id";
        p3.Value = 10;
        sel.Parameters.Add(p3);
        var result = sel.ExecuteScalar();
        Assert.Equal("test", result);
    }

    [Fact]
    public void Connection_SetToProfileDbConnection_InnerCommandUsesInnerConnection()
    {
        using var con = CreateConnection();

        using var cmd = con.CreateCommand();
        // Setting Connection to the same ProfileDbConnection should not break the inner cmd
        cmd.Connection = con;

        cmd.CommandText = "SELECT 42";
        var result = cmd.ExecuteScalar();
        Assert.Equal(42L, result);
    }

    [Fact]
    public void Transaction_SetToProfileDbTransaction_IsUnwrapped()
    {
        using var con = CreateConnection();

        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE txtest (val INTEGER)";
        create.ExecuteNonQuery();

        using var tx = (ProfileDbTransaction)con.BeginTransaction();

        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO txtest VALUES (99)";
        ins.Transaction = tx;
        ins.ExecuteNonQuery();

        tx.Commit();

        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT val FROM txtest";
        var result = sel.ExecuteScalar();
        Assert.Equal(99L, result);
    }

    [Fact]
    public void Transaction_Commit_PersistsData()
    {
        using var con = CreateConnection();
        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE txdata (val INTEGER)";
        create.ExecuteNonQuery();

        using var tx = con.BeginTransaction();
        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO txdata VALUES (7)";
        ins.Transaction = tx;
        ins.ExecuteNonQuery();
        tx.Commit();

        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT COUNT(*) FROM txdata";
        Assert.Equal(1L, sel.ExecuteScalar());
    }

    [Fact]
    public void Transaction_Rollback_RevertsData()
    {
        using var con = CreateConnection();
        using var create = con.CreateCommand();
        create.CommandText = "CREATE TABLE txdata2 (val INTEGER)";
        create.ExecuteNonQuery();

        using var tx = con.BeginTransaction();
        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO txdata2 VALUES (7)";
        ins.Transaction = tx;
        ins.ExecuteNonQuery();
        tx.Rollback();

        using var sel = con.CreateCommand();
        sel.CommandText = "SELECT COUNT(*) FROM txdata2";
        Assert.Equal(0L, sel.ExecuteScalar());
    }
}
