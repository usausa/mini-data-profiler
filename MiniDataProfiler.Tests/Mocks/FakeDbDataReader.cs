namespace MiniDataProfiler.Mocks;

using System.Data.Common;

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

    public override System.Collections.IEnumerator GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();
}
