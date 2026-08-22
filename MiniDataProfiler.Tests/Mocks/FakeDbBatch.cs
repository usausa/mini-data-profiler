namespace MiniDataProfiler.Mocks;

using System.Data;
using System.Data.Common;

internal sealed class FakeDbBatch : DbBatch
{
    private readonly bool fail;

    private readonly FakeDbBatchCommandCollection commands = [];

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

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        if (fail)
        {
            throw new InvalidOperationException("FakeDbBatch forced failure");
        }

        return new FakeDbDataReader();
    }

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        if (fail)
        {
            throw new InvalidOperationException("FakeDbBatch forced failure");
        }

        return Task.FromResult<DbDataReader>(new FakeDbDataReader());
    }

    public override void Cancel()
    {
    }

    public override void Prepare()
    {
    }

    public override Task PrepareAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public override object? ExecuteScalar() => null;

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);

    protected override DbBatchCommand CreateDbBatchCommand() => new FakeDbBatchCommand();
}
