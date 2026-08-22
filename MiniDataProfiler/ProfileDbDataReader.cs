namespace MiniDataProfiler;

using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

internal abstract class ProfileDbDataReader : DbDataReader, IDbColumnSchemaGenerator
{
    private readonly DbDataReader reader;

    private readonly long start;

    private int recordsRead;

    private bool finished;

    protected ProfileDbDataReader(DbDataReader reader, long start)
    {
        this.reader = reader;
        this.start = start;
    }

    protected abstract void OnFinished(int recordsRead, TimeSpan duration);

    private void Finish()
    {
        if (finished)
        {
            return;
        }

        finished = true;

        OnFinished(recordsRead, Stopwatch.GetElapsedTime(start));
    }

    // Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            reader.Dispose();
            Finish();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await reader.DisposeAsync().ConfigureAwait(false);
        Finish();
        await base.DisposeAsync().ConfigureAwait(false);
    }

    // Read

    public override bool Read()
    {
        var result = reader.Read();
        if (result)
        {
            recordsRead++;
        }

        return result;
    }

    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (result)
        {
            recordsRead++;
        }

        return result;
    }

    public override bool NextResult() => reader.NextResult();

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => reader.NextResultAsync(cancellationToken);

    // Close

    public override void Close() => reader.Close();

    public override Task CloseAsync() => reader.CloseAsync();

    // State

    public override int Depth => reader.Depth;

    public override int FieldCount => reader.FieldCount;

    public override bool HasRows => reader.HasRows;

    public override bool IsClosed => reader.IsClosed;

    public override int RecordsAffected => reader.RecordsAffected;

    public override int VisibleFieldCount => reader.VisibleFieldCount;

    public override object this[int ordinal] => reader[ordinal];

    public override object this[string name] => reader[name];

    // Metadata

    public override string GetName(int ordinal) => reader.GetName(ordinal);

    public override int GetOrdinal(string name) => reader.GetOrdinal(name);

    public override string GetDataTypeName(int ordinal) => reader.GetDataTypeName(ordinal);

    public override Type GetFieldType(int ordinal) => reader.GetFieldType(ordinal);

    public override Type GetProviderSpecificFieldType(int ordinal) => reader.GetProviderSpecificFieldType(ordinal);

    public override DataTable? GetSchemaTable() => reader.GetSchemaTable();

    public ReadOnlyCollection<DbColumn> GetColumnSchema() => reader.GetColumnSchema();

    // Get value

    public override bool GetBoolean(int ordinal) => reader.GetBoolean(ordinal);

    public override byte GetByte(int ordinal) => reader.GetByte(ordinal);

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => reader.GetChar(ordinal);

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

    public override DateTime GetDateTime(int ordinal) => reader.GetDateTime(ordinal);

    public override decimal GetDecimal(int ordinal) => reader.GetDecimal(ordinal);

    public override double GetDouble(int ordinal) => reader.GetDouble(ordinal);

    public override float GetFloat(int ordinal) => reader.GetFloat(ordinal);

    public override Guid GetGuid(int ordinal) => reader.GetGuid(ordinal);

    public override short GetInt16(int ordinal) => reader.GetInt16(ordinal);

    public override int GetInt32(int ordinal) => reader.GetInt32(ordinal);

    public override long GetInt64(int ordinal) => reader.GetInt64(ordinal);

    public override string GetString(int ordinal) => reader.GetString(ordinal);

    public override object GetValue(int ordinal) => reader.GetValue(ordinal);

    public override int GetValues(object[] values) => reader.GetValues(values);

    public override object GetProviderSpecificValue(int ordinal) => reader.GetProviderSpecificValue(ordinal);

    public override int GetProviderSpecificValues(object[] values) => reader.GetProviderSpecificValues(values);

    public override T GetFieldValue<T>(int ordinal) => reader.GetFieldValue<T>(ordinal);

    public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) =>
        reader.GetFieldValueAsync<T>(ordinal, cancellationToken);

    public override Stream GetStream(int ordinal) => reader.GetStream(ordinal);

    public override TextReader GetTextReader(int ordinal) => reader.GetTextReader(ordinal);

    public override bool IsDBNull(int ordinal) => reader.IsDBNull(ordinal);

    public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) =>
        reader.IsDBNullAsync(ordinal, cancellationToken);

    protected override DbDataReader GetDbDataReader(int ordinal) => reader.GetData(ordinal);

    public override IEnumerator GetEnumerator() => new DbEnumerator(this, false);
}

internal sealed class ProfileCommandDbDataReader : ProfileDbDataReader
{
    private readonly IProfileListener listener;

    private readonly DbCommand command;

    private readonly EventType eventType;

    public ProfileCommandDbDataReader(IProfileListener listener, DbCommand command, DbDataReader reader, EventType eventType, long start)
        : base(reader, start)
    {
        this.listener = listener;
        this.command = command;
        this.eventType = eventType;
    }

    protected override void OnFinished(int recordsRead, TimeSpan duration)
    {
        var context = new ProfilerReaderFinishedContext(eventType, command, recordsRead, duration);
        listener.ReaderFinished(in context);
    }
}

internal sealed class ProfileBatchDbDataReader : ProfileDbDataReader
{
    private readonly IProfileListener listener;

    private readonly DbBatch batch;

    private readonly EventType eventType;

    public ProfileBatchDbDataReader(IProfileListener listener, DbBatch batch, DbDataReader reader, EventType eventType, long start)
        : base(reader, start)
    {
        this.listener = listener;
        this.batch = batch;
        this.eventType = eventType;
    }

    protected override void OnFinished(int recordsRead, TimeSpan duration)
    {
        var context = new BatchProfilerReaderFinishedContext(eventType, batch, recordsRead, duration);
        listener.BatchReaderFinished(in context);
    }
}
