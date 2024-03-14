namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;

// ReSharper disable ConvertToAutoProperty
#pragma warning disable IDE0032
public sealed class ProfileDbTransaction : DbTransaction
{
    private readonly ProfileDbConnection con;

    private readonly DbTransaction tx;

    internal DbTransaction InnerTransaction => tx;

    public override bool SupportsSavepoints => tx.SupportsSavepoints;

    public ProfileDbTransaction(ProfileDbConnection con, DbTransaction tx)
    {
        this.con = con;
        this.tx = tx;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            tx.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override DbConnection DbConnection => con;

    public override IsolationLevel IsolationLevel => tx.IsolationLevel;

    public override void Commit() => tx.Commit();

    public override Task CommitAsync(CancellationToken cancellationToken = default) => tx.CommitAsync(cancellationToken);

    public override void Rollback() => tx.Rollback();

    public override Task RollbackAsync(CancellationToken cancellationToken = default) => tx.RollbackAsync(cancellationToken);

    public override void Save(string savepointName) => tx.Save(savepointName);

    public override Task SaveAsync(string savepointName, CancellationToken cancellationToken = default) => tx.SaveAsync(savepointName, cancellationToken);

    public override void Rollback(string savepointName) => tx.Rollback(savepointName);

    public override Task RollbackAsync(string savepointName, CancellationToken cancellationToken = default) => tx.RollbackAsync(savepointName, cancellationToken);

    public override void Release(string savepointName) => tx.Release(savepointName);

    public override Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default) => tx.ReleaseAsync(savepointName, cancellationToken);
}
