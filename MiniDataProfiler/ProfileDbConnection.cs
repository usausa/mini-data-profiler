namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

// ReSharper disable ConvertToAutoProperty
#pragma warning disable IDE0032
public sealed class ProfileDbConnection : DbConnection
{
    private readonly IProfileListener listener;

    private readonly DbConnection con;

    public DbConnection InnerConnection => con;

    [AllowNull]
    public override string ConnectionString
    {
        get => con.ConnectionString;
        set => con.ConnectionString = value;
    }

    public override int ConnectionTimeout => con.ConnectionTimeout;

    public override string Database => con.Database;

    public override string DataSource => con.DataSource;

    public override string ServerVersion => con.ServerVersion;

    public override ConnectionState State => con.State;

    // TODO Batch

    public override bool CanCreateBatch => false; //con.CanCreateBatch;

    public ProfileDbConnection(IProfileListener listener, DbConnection con)
    {
        this.listener = listener;
        this.con = con;

        con.StateChange += OnStateChange;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            con.StateChange -= OnStateChange;
            con.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnStateChange(object sender, StateChangeEventArgs e)
    {
        OnStateChange(e);
    }

    // Open/Close

    public override void Open()
    {
        con.Open();
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        return con.OpenAsync(cancellationToken);
    }

    public override void Close()
    {
        con.Close();
    }

    public override Task CloseAsync()
    {
        return con.CloseAsync();
    }

    // ChangeDatabase

    public override void ChangeDatabase(string databaseName) =>
        con.ChangeDatabase(databaseName);

    public override Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default) =>
        con.ChangeDatabaseAsync(databaseName, cancellationToken);

    // Scheme

    public override DataTable GetSchema() =>
        con.GetSchema();

    public override DataTable GetSchema(string collectionName) =>
        con.GetSchema(collectionName);

    public override DataTable GetSchema(string collectionName, string?[] restrictionValues) =>
        con.GetSchema(collectionName, restrictionValues);

    public override Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default) =>
        con.GetSchemaAsync(cancellationToken);

    public override Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default) =>
        con.GetSchemaAsync(collectionName, cancellationToken);

    public override Task<DataTable> GetSchemaAsync(string collectionName, string?[] restrictionValues, CancellationToken cancellationToken = default) =>
        con.GetSchemaAsync(collectionName, restrictionValues, cancellationToken);

    // Transaction

    protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) =>
        new ProfileDbTransaction(this, con.BeginTransaction(isolationLevel));

    protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken) =>
        new ProfileDbTransaction(this, await con.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false));

    public override void EnlistTransaction(Transaction? transaction) => con.EnlistTransaction(transaction);

    // Command

    protected override DbCommand CreateDbCommand() => new ProfileDbCommand(listener, this, con.CreateCommand());

    // TODO Batch

    protected override DbBatch CreateDbBatch() => throw new NotSupportedException();
}
#pragma warning restore IDE0032
// ReSharper restore ConvertToAutoProperty
