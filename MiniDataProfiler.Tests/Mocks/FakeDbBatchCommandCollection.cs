namespace MiniDataProfiler.Mocks;

using System.Data.Common;

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
