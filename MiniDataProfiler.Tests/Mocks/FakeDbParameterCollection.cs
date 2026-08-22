namespace MiniDataProfiler.Mocks;

using System.Data.Common;

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
