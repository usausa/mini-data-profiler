namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;

using Microsoft.Data.Sqlite;

public sealed class BatchTests
{
    // SQLite supports CanCreateBatch from Microsoft.Data.Sqlite 8+
    private static bool SqliteCanCreateBatch()
    {
        using var con = new SqliteConnection("Data Source=:memory:");
        return con.CanCreateBatch;
    }

    private static ProfileDbConnection CreateConnection(IProfileListener listener)
    {
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite);
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
    public void BatchNonQuerySync_EventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return; // skip: SQLite doesn't support batches on this platform
        }

        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "INSERT INTO t VALUES (2, 'world')";
        batch.BatchCommands.Add(batchCmd);

        var result = batch.ExecuteNonQuery();

        Assert.Equal(1, result);
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchNonQueryExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task BatchNonQueryAsync_EventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "INSERT INTO t VALUES (3, 'async')";
        batch.BatchCommands.Add(batchCmd);

        var result = await batch.ExecuteNonQueryAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal(1, result);
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchNonQueryExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void BatchReaderSync_EventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        batch.BatchCommands.Add(batchCmd);

        using var reader = batch.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));

        Assert.Equal([nameof(IProfileListener.BatchReaderExecuting), nameof(IProfileListener.BatchReaderExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task BatchReaderAsync_EventsAndResult()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT id, val FROM t WHERE id = 1";
        batch.BatchCommands.Add(batchCmd);

        using var reader = await batch.ExecuteReaderAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken).ConfigureAwait(true));
        Assert.Equal(1L, reader.GetInt64(0));
        Assert.Equal("hello", reader.GetString(1));

        Assert.Equal([nameof(IProfileListener.BatchReaderExecuting), nameof(IProfileListener.BatchReaderExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void BatchNonQuery_Failed_EventsAndDuration()
    {
        if (!SqliteCanCreateBatch())
        {
            return;
        }

        var listener = new RecordingListener();
        using var con = CreateConnection(listener);

        listener.Events.Clear();
        using var batch = con.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "INVALID SQL";
        batch.BatchCommands.Add(batchCmd);

        Assert.Throws<SqliteException>(() => batch.ExecuteNonQuery());
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchFailed), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<SqliteException>(listener.LastFailedException);
    }

    // Fake batch tests when SQLite does not support CanCreateBatch
    // (provides coverage of ProfileDbBatch event routing via a fake DbConnection)

    [Fact]
    public void FakeBatch_NonQuerySync_EventsAndResult()
    {
        var listener = new RecordingListener();
        var innerCon = new FakeInnerBatchDbConnection(failBatch: false);
        using var profileCon = new ProfileDbConnection(listener, innerCon);
        profileCon.Open();

        listener.Events.Clear();
        using var batch = profileCon.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT 1";
        batch.BatchCommands.Add(batchCmd);

        var result = batch.ExecuteNonQuery();

        Assert.Equal(1, result);
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchNonQueryExecuted), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastExecutedDuration >= TimeSpan.Zero);
    }

    [Fact]
    public void FakeBatch_Failed_EventsAndDuration()
    {
        var listener = new RecordingListener();
        var innerCon = new FakeInnerBatchDbConnection(failBatch: true);
        using var profileCon = new ProfileDbConnection(listener, innerCon);
        profileCon.Open();

        listener.Events.Clear();
        using var batch = profileCon.CreateBatch();
        var batchCmd = batch.CreateBatchCommand();
        batchCmd.CommandText = "SELECT 1";
        batch.BatchCommands.Add(batchCmd);

        Assert.Throws<InvalidOperationException>(() => batch.ExecuteNonQuery());
        Assert.Equal([nameof(IProfileListener.BatchNonQueryExecuting), nameof(IProfileListener.BatchFailed), nameof(IProfileListener.BatchFinally)], listener.Events);
        Assert.True(listener.LastFailedDuration >= TimeSpan.Zero);
        Assert.IsType<InvalidOperationException>(listener.LastFailedException);
    }
}

// -----------------------------------------------------------------
// FakeInnerBatchDbConnection: inner DbConnection that always CanCreateBatch = true
// Returns a FakeDbBatch from CreateDbBatch()
// -----------------------------------------------------------------

internal sealed class FakeInnerBatchDbConnection : DbConnection
{
    private readonly bool failBatch;

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    public override int ConnectionTimeout => 0;

    public override string Database => string.Empty;

    public override string DataSource => string.Empty;

    public override string ServerVersion => string.Empty;

    public override System.Data.ConnectionState State => System.Data.ConnectionState.Open;

    public override bool CanCreateBatch => true;

    public FakeInnerBatchDbConnection(bool failBatch = false)
    {
        this.failBatch = failBatch;
    }

    public override void Open() { }

    public override Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override void Close() { }

    public override void ChangeDatabase(string databaseName) { }

    protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) =>
        throw new NotSupportedException();

    protected override DbCommand CreateDbCommand() => throw new NotSupportedException();

    protected override DbBatch CreateDbBatch() => new FakeDbBatch(failBatch);
}

internal sealed class FakeDbBatch : DbBatch
{
    private readonly bool fail;

    private readonly FakeDbBatchCommandCollection commands = new();

    protected override DbBatchCommandCollection DbBatchCommands => commands;

    public override int Timeout { get; set; }

    protected override DbConnection? DbConnection { get; set; }

    protected override DbTransaction? DbTransaction { get; set; }

    public FakeDbBatch(bool fail = false)
    {
        this.fail = fail;
    }

    public override int ExecuteNonQuery()
    {
        if (fail)
        {
            throw new InvalidOperationException("FakeDbBatch forced failure");
        }

        return 1;
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
    {
        if (fail)
        {
            throw new InvalidOperationException("FakeDbBatch forced failure");
        }

        return Task.FromResult(1);
    }

    protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
    {
        if (fail)
        {
            throw new InvalidOperationException("FakeDbBatch forced failure");
        }

        return new FakeDbDataReader();
    }

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(System.Data.CommandBehavior behavior, CancellationToken cancellationToken)
    {
        if (fail)
        {
            throw new InvalidOperationException("FakeDbBatch forced failure");
        }

        return Task.FromResult<DbDataReader>(new FakeDbDataReader());
    }

    public override void Cancel() { }

    public override void Prepare() { }

    public override Task PrepareAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public override object? ExecuteScalar() => null;

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);

    protected override DbBatchCommand CreateDbBatchCommand() => new FakeDbBatchCommand();
}

internal sealed class FakeDbBatchCommandCollection : DbBatchCommandCollection
{
    private readonly List<DbBatchCommand> items = [];

    public override int Count => items.Count;

    public override bool IsReadOnly => false;

    public override void Add(DbBatchCommand item) => items.Add(item);

    public override void Clear() => items.Clear();

    public override bool Contains(DbBatchCommand item) => items.Contains(item);

    public override void CopyTo(DbBatchCommand[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

    public override IEnumerator<DbBatchCommand> GetEnumerator() => items.GetEnumerator();

    public override int IndexOf(DbBatchCommand item) => items.IndexOf(item);

    public override void Insert(int index, DbBatchCommand item) => items.Insert(index, item);

    public override bool Remove(DbBatchCommand item) => items.Remove(item);

    public override void RemoveAt(int index) => items.RemoveAt(index);

    protected override DbBatchCommand GetBatchCommand(int index) => items[index];

    protected override void SetBatchCommand(int index, DbBatchCommand batchCommand) => items[index] = batchCommand;
}

internal sealed class FakeDbBatchCommand : DbBatchCommand
{
    public override string CommandText { get; set; } = string.Empty;

    public override System.Data.CommandType CommandType { get; set; }

    public override int RecordsAffected => 0;

    protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();
}

internal sealed class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> items = [];

    public override int Count => items.Count;

    public override object SyncRoot => items;

    public override int Add(object value)
    {
        items.Add((DbParameter)value);
        return items.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (DbParameter p in values)
        {
            items.Add(p);
        }
    }

    public override void Clear() => items.Clear();

    public override bool Contains(object value) => items.Contains((DbParameter)value);

    public override bool Contains(string value) => items.Any(p => p.ParameterName == value);

    public override void CopyTo(Array array, int index) => ((System.Collections.IList)items).CopyTo(array, index);

    public override System.Collections.IEnumerator GetEnumerator() => items.GetEnumerator();

    public override int IndexOf(object value) => items.IndexOf((DbParameter)value);

    public override int IndexOf(string parameterName) => items.FindIndex(p => p.ParameterName == parameterName);

    public override void Insert(int index, object value) => items.Insert(index, (DbParameter)value);

    public override void Remove(object value) => items.Remove((DbParameter)value);

    public override void RemoveAt(int index) => items.RemoveAt(index);

    public override void RemoveAt(string parameterName)
    {
        var idx = IndexOf(parameterName);
        if (idx >= 0)
        {
            items.RemoveAt(idx);
        }
    }

    protected override DbParameter GetParameter(int index) => items[index];

    protected override DbParameter GetParameter(string parameterName) => items.First(p => p.ParameterName == parameterName);

    protected override void SetParameter(int index, DbParameter value) => items[index] = value;

    protected override void SetParameter(string parameterName, DbParameter value)
    {
        var idx = IndexOf(parameterName);
        if (idx >= 0)
        {
            items[idx] = value;
        }
    }
}

internal sealed class FakeDbDataReader : DbDataReader
{
    public override int FieldCount => 0;

    public override int RecordsAffected => 0;

    public override bool HasRows => false;

    public override bool IsClosed => false;

    public override int Depth => 0;

    public override bool Read() => false;

    public override bool NextResult() => false;

    public override bool GetBoolean(int ordinal) => false;

    public override byte GetByte(int ordinal) => 0;

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0;

    public override char GetChar(int ordinal) => '\0';

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0;

    public override string GetDataTypeName(int ordinal) => string.Empty;

    public override DateTime GetDateTime(int ordinal) => DateTime.MinValue;

    public override decimal GetDecimal(int ordinal) => 0;

    public override double GetDouble(int ordinal) => 0;

    public override Type GetFieldType(int ordinal) => typeof(object);

    public override float GetFloat(int ordinal) => 0;

    public override Guid GetGuid(int ordinal) => Guid.Empty;

    public override short GetInt16(int ordinal) => 0;

    public override int GetInt32(int ordinal) => 0;

    public override long GetInt64(int ordinal) => 0;

    public override string GetName(int ordinal) => string.Empty;

    public override int GetOrdinal(string name) => -1;

    public override string GetString(int ordinal) => string.Empty;

    public override object GetValue(int ordinal) => DBNull.Value;

    public override int GetValues(object[] values) => 0;

    public override bool IsDBNull(int ordinal) => true;

    public override object this[int ordinal] => DBNull.Value;

    public override object this[string name] => DBNull.Value;

    public override System.Collections.IEnumerator GetEnumerator() => System.Linq.Enumerable.Empty<object>().GetEnumerator();
}
