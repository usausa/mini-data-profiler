namespace MiniDataProfiler.Mocks;

using System.Data;
using System.Data.Common;

internal sealed class FakeInnerBatchDbConnection : DbConnection
{
    private readonly bool failBatch;

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    public override int ConnectionTimeout => 0;

    public override string Database => string.Empty;

    public override string DataSource => string.Empty;

    public override string ServerVersion => string.Empty;

    public override ConnectionState State => ConnectionState.Open;

    public override bool CanCreateBatch => true;

    public FakeInnerBatchDbConnection(bool failBatch = false)
    {
        this.failBatch = failBatch;
    }

    public override void Open()
    {
    }

    public override Task OpenAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override void Close()
    {
    }

    public override void ChangeDatabase(string databaseName)
    {
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
        throw new NotSupportedException();

    protected override DbCommand CreateDbCommand() => throw new NotSupportedException();

    protected override DbBatch CreateDbBatch() => new FakeDbBatch(failBatch);
}
