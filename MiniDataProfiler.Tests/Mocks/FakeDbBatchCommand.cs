namespace MiniDataProfiler.Mocks;

using System.Data;
using System.Data.Common;

internal sealed class FakeDbBatchCommand : DbBatchCommand
{
    public override string CommandText { get; set; } = string.Empty;

    public override CommandType CommandType { get; set; }

    public override int RecordsAffected => 0;

    protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();
}
