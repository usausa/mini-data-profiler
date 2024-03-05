namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

// ReSharper disable ConvertToAutoProperty
#pragma warning disable IDE0032
public sealed class ProfileDbConnection : DbConnection
{
    private readonly IProfileExporter exporter;

    private readonly DbConnection con;

    internal DbConnection InnerConnection => con;

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

    public ProfileDbConnection(IProfileExporter exporter, DbConnection con)
    {
        this.exporter = exporter;
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
        // TODO watch & report ?
        con.Open();
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        // TODO watch & report ?
        return con.OpenAsync(cancellationToken);
    }

    public override void Close()
    {
        // TODO watch & report ?
        con.Close();
    }

    public override Task CloseAsync()
    {
        // TODO watch & report
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
        con.BeginTransaction(isolationLevel);

    protected override ValueTask<DbTransaction> BeginDbTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken) =>
        con.BeginTransactionAsync(isolationLevel, cancellationToken);

    public override void EnlistTransaction(Transaction? transaction) => con.EnlistTransaction(transaction);

    // Command

    protected override DbCommand CreateDbCommand() => new ProfileDbCommand(exporter, this, con.CreateCommand());

    // TODO Batch

    protected override DbBatch CreateDbBatch() => throw new NotSupportedException();
}
#pragma warning restore IDE0032
// ReSharper restore ConvertToAutoProperty
