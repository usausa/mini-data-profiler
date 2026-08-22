namespace Example.Api.Data;

using System.Globalization;

using Microsoft.Data.Sqlite;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var schema = connection.CreateCommand();
        schema.CommandText =
            """
            PRAGMA journal_mode = WAL;
            CREATE TABLE IF NOT EXISTS Data (
                Id   INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Type TEXT NOT NULL
            );
            """;
        await schema.ExecuteNonQueryAsync(cancellationToken);

        await using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Data";
        var existing = Convert.ToInt64(await count.ExecuteScalarAsync(cancellationToken) ?? 0L, CultureInfo.InvariantCulture);
        if (existing == 0)
        {
            await using var seed = connection.CreateCommand();
            seed.CommandText =
                """
                INSERT INTO Data (Name, Type) VALUES ('Data-1', 'A');
                INSERT INTO Data (Name, Type) VALUES ('Data-2', 'B');
                INSERT INTO Data (Name, Type) VALUES ('Data-3', 'A');
                """;
            await seed.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
