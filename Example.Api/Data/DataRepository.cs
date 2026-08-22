namespace Example.Api.Data;

using System.Data.Common;
using System.Globalization;

using Example.Api.Models;

using MiniDataProfiler;

// ExecuteReader   -> ReaderExecuting / ReaderExecuted / CommandFinally (+ ReaderFinished)
// ExecuteScalar   -> ScalarExecuting / ScalarExecuted / CommandFinally
// ExecuteNonQuery -> NonQueryExecuting / NonQueryExecuted / CommandFinally
// (failure)       -> CommandFailed / CommandFinally
public sealed class DataRepository
{
    private readonly ProfileDbDataSource dataSource;

    public DataRepository(ProfileDbDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<IReadOnlyList<DataEntity>> QueryAllAsync(string? type, CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        if (String.IsNullOrEmpty(type))
        {
            command.CommandText = "SELECT Id, Name, Type FROM Data ORDER BY Id";
        }
        else
        {
            command.CommandText = "SELECT Id, Name, Type FROM Data WHERE Type = $type ORDER BY Id";
            AddParameter(command, "$type", type);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await ReadEntitiesAsync(reader, cancellationToken);
    }

    public IReadOnlyList<DataEntity> QueryAllSync()
    {
        using var connection = dataSource.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Type FROM Data ORDER BY Id";

        using var reader = command.ExecuteReader();
        var list = new List<DataEntity>();
        while (reader.Read())
        {
            list.Add(MapEntity(reader));
        }

        return list;
    }

    public async Task<DataEntity?> FindAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Type FROM Data WHERE Id = $id";
        AddParameter(command, "$id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapEntity(reader) : null;
    }

    public async Task<long> CountAsync(CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Data";
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken) ?? 0L, CultureInfo.InvariantCulture);
    }

    public async Task<int> InsertAsync(CreateItemRequest request, CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Data (Name, Type) VALUES ($name, $type)";
        AddParameter(command, "$name", request.Name);
        AddParameter(command, "$type", request.Type);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(long id, UpdateItemRequest request, CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Data SET Name = $name, Type = $type WHERE Id = $id";
        AddParameter(command, "$name", request.Name);
        AddParameter(command, "$type", request.Type);
        AddParameter(command, "$id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Data WHERE Id = $id";
        AddParameter(command, "$id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> TransactionAsync(bool commit, CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO Data (Name, Type) VALUES ($name, $type)";
        AddParameter(command, "$name", commit ? "Tx-Commit" : "Tx-Rollback");
        AddParameter(command, "$type", "T");
        await command.ExecuteNonQueryAsync(cancellationToken);

        if (commit)
        {
            await transaction.CommitAsync(cancellationToken);
            return 1;
        }

        await transaction.RollbackAsync(cancellationToken);
        return 0;
    }

    public async Task<IReadOnlyList<DataEntity>> QueryViaDataSourceCommandAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("SELECT Id, Name, Type FROM Data ORDER BY Id");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await ReadEntitiesAsync(reader, cancellationToken);
    }

    public async Task QueryInvalidAsync(CancellationToken cancellationToken)
    {
        await using var connection = dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM NoSuchTable";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<DataEntity>> ReadEntitiesAsync(DbDataReader reader, CancellationToken cancellationToken)
    {
        var list = new List<DataEntity>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(MapEntity(reader));
        }

        return list;
    }

    private static DataEntity MapEntity(DbDataReader reader) => new()
    {
        Id = reader.GetInt64(0),
        Name = reader.GetString(1),
        Type = reader.GetString(2)
    };

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
