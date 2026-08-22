using System.Data.Common;
using System.Globalization;

using Example.Batch;

using Microsoft.Extensions.Logging;

using MiniDataProfiler;
using MiniDataProfiler.Listener.Logging;

using Npgsql;

var connectionString = args.Length > 0 ? args[0] : "Host=postgres-server;Database=test;Username=test;Password=test";

using var loggerFactory = LoggerFactory.Create(builder => builder
    .AddFilter("MiniDataProfiler.Listener.Logging", LogLevel.Information)
    .AddSimpleConsole(options => options.SingleLine = true));

var recording = new RecordingListener();
var listener = new ChainListener(
    new LoggingListener(
        loggerFactory.CreateLogger<LoggingListener>(),
        new LoggingListenerOption
        {
            OutputStartLog = true,
            OutputFinallyLog = true,
            OutputExceptionLog = true,
            OutputParameter = true
        }),
    recording);

Console.WriteLine($"Connecting to: {connectionString}");

await using var npgsql = new NpgsqlConnection(connectionString);
await using var connection = new ProfileDbConnection(listener, npgsql, new ProfilerOption { WrapDataReader = true });

#pragma warning disable CA1031
try
{
    await connection.OpenAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Could not connect ({ex.GetType().Name}: {ex.Message}).");
    Console.WriteLine(String.Create(CultureInfo.InvariantCulture, $"Ensure a PostgreSQL server is reachable with the connection string above."));
    return 2;
}
#pragma warning restore CA1031

Console.WriteLine($"Connected. ServerVersion={connection.ServerVersion}, CanCreateBatch={connection.CanCreateBatch}");
Console.WriteLine();

await using var setup = connection.CreateCommand();
setup.CommandText =
    "DROP TABLE IF EXISTS example_batch;" +
    "CREATE TABLE example_batch (id serial primary key, name text not null, type text not null);";
await setup.ExecuteNonQueryAsync();

var verifier = new Verifier();

// ---------------------------------------------------------------------------
// Scenario 1: batch ExecuteNonQuery
// ---------------------------------------------------------------------------
recording.Clear();
await using (var batch = connection.CreateBatch())
{
    AddInsert(batch, "Batch-1", "A");
    AddInsert(batch, "Batch-2", "B");
    AddInsert(batch, "Batch-3", "A");

    var affected = await batch.ExecuteNonQueryAsync();

    verifier.Check(
        "Batch ExecuteNonQuery",
        (affected == 3) && recording.Events.SequenceEqual(
        [
            nameof(IProfileListener.BatchNonQueryExecuting),
            nameof(IProfileListener.BatchNonQueryExecuted),
            nameof(IProfileListener.BatchFinally)
        ]),
        $"affected={affected}, events=[{String.Join(", ", recording.Events)}]");
}

// ---------------------------------------------------------------------------
// Scenario 2: batch ExecuteReader
// ---------------------------------------------------------------------------
recording.Clear();
await using (var batch = connection.CreateBatch())
{
    AddSelect(batch, "A");
    AddSelect(batch, "B");

    var rows = 0;
    await using (var reader = await batch.ExecuteReaderAsync())
    {
        do
        {
            while (await reader.ReadAsync())
            {
                rows++;
            }
        }
        while (await reader.NextResultAsync());
    }

    verifier.Check(
        "Batch ExecuteReader",
        (rows == 3) &&
        recording.Events.Contains(nameof(IProfileListener.BatchReaderExecuting)) &&
        recording.Events.Contains(nameof(IProfileListener.BatchReaderExecuted)) &&
        recording.Events.Contains(nameof(IProfileListener.BatchReaderFinished)) &&
        (recording.LastRecordsRead == 3),
        $"rows={rows}, recordsRead={recording.LastRecordsRead}, events=[{String.Join(", ", recording.Events)}]");
}

// ---------------------------------------------------------------------------
// Scenario 3: batch failure (invalid SQL -> BatchFailed)
// ---------------------------------------------------------------------------
recording.Clear();
await using (var batch = connection.CreateBatch())
{
    var command = batch.CreateBatchCommand();
    command.CommandText = "INSERT INTO no_such_table VALUES (1)";
    batch.BatchCommands.Add(command);

    var threw = false;
    try
    {
        await batch.ExecuteNonQueryAsync();
    }
    catch (DbException)
    {
        threw = true;
    }

    verifier.Check(
        "Batch failure",
        threw && recording.Events.SequenceEqual([
            nameof(IProfileListener.BatchNonQueryExecuting),
            nameof(IProfileListener.BatchFailed),
            nameof(IProfileListener.BatchFinally)
        ]),
        $"threw={threw}, events=[{String.Join(", ", recording.Events)}]");
}

await using var cleanup = connection.CreateCommand();
cleanup.CommandText = "DROP TABLE IF EXISTS example_batch";
await cleanup.ExecuteNonQueryAsync();

Console.WriteLine();
Console.WriteLine(String.Create(CultureInfo.InvariantCulture, $"RESULT: {(verifier.AllPassed ? "all batch scenarios passed." : "some batch scenarios FAILED.")}"));
return verifier.AllPassed ? 0 : 1;

static void AddInsert(DbBatch batch, string name, string type)
{
    var command = batch.CreateBatchCommand();
    command.CommandText = "INSERT INTO example_batch (name, type) VALUES (@name, @type)";
    command.Parameters.Add(new NpgsqlParameter("name", name));
    command.Parameters.Add(new NpgsqlParameter("type", type));
    batch.BatchCommands.Add(command);
}

static void AddSelect(DbBatch batch, string type)
{
    var command = batch.CreateBatchCommand();
    command.CommandText = "SELECT id, name, type FROM example_batch WHERE type = @type ORDER BY id";
    command.Parameters.Add(new NpgsqlParameter("type", type));
    batch.BatchCommands.Add(command);
}
