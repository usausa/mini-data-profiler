// ReSharper disable ExplicitCallerInfoArgument
namespace MiniDataProfiler;

using System.Diagnostics;

using Microsoft.Data.Sqlite;

using MiniDataProfiler.Listener.OpenTelemetry;

public sealed class OpenTelemetryListenerTests
{
    private const string SourceName = "MiniDataProfiler.Listener.OpenTelemetry";

    private const string RequestSourceName = "Test.Request";

    private static ProfileDbConnection CreateConnection(IProfileListener listener, bool wrapReader)
    {
#pragma warning disable CA2000
        var sqlite = new SqliteConnection("Data Source=:memory:");
        var con = new ProfileDbConnection(listener, sqlite, new ProfilerOption { WrapDataReader = wrapReader });
#pragma warning restore CA2000
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "CREATE TABLE t (id INTEGER PRIMARY KEY, val TEXT)";
        cmd.ExecuteNonQuery();

        using var ins = con.CreateCommand();
        ins.CommandText = "INSERT INTO t VALUES (1, 'v1'), (2, 'v2'), (3, 'v3')";
        ins.ExecuteNonQuery();

        return con;
    }

    [Fact]
    public void ReaderWrappedEmitsSingleActivity()
    {
        // Arrange
        using var recorder = new ActivityRecorder(SourceName);
        using var listener = new OpenTelemetryListener(new OpenTelemetryListenerOption());
        using var con = CreateConnection(listener, wrapReader: true);

        recorder.Activities.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
            }
        }

        // Assert
        var activity = Assert.Single(recorder.Activities);
        Assert.Equal(nameof(EventType.ExecuteReader), activity.DisplayName);
        Assert.Equal("SELECT id, val FROM t ORDER BY id", activity.GetTagItem("db.query.text"));
        Assert.Equal(3, activity.GetTagItem("db.response.rows_read"));
    }

    [Fact]
    public async Task ReaderAsyncWrappedEmitsSingleActivity()
    {
        // Arrange
        using var recorder = new ActivityRecorder(SourceName);
        using var listener = new OpenTelemetryListener(new OpenTelemetryListenerOption());
        await using var con = CreateConnection(listener, wrapReader: true);

        recorder.Activities.Clear();

        // Act
        await ReadAllAsync(con);

        // Assert
        var activity = Assert.Single(recorder.Activities);
        Assert.Equal(nameof(EventType.ExecuteReaderAsync), activity.DisplayName);
        Assert.Equal(3, activity.GetTagItem("db.response.rows_read"));
    }

    [Fact]
    public async Task ReaderAsyncWrappedNestsSingleChildUnderRequestActivity()
    {
        // Arrange
        using var requestSource = new ActivitySource(RequestSourceName);
        using var recorder = new ActivityRecorder(SourceName, RequestSourceName);
        using var listener = new OpenTelemetryListener(new OpenTelemetryListenerOption());
        await using var con = CreateConnection(listener, wrapReader: true);

        recorder.Activities.Clear();

        // Act
        using (requestSource.StartActivity("GET /api/items", ActivityKind.Server))
        {
            await ReadAllAsync(con);
        }

        // Assert
        Assert.Equal(2, recorder.Activities.Count);

        var request = Assert.Single(recorder.Activities, x => x.DisplayName == "GET /api/items");
        var sql = Assert.Single(recorder.Activities, x => x.DisplayName == nameof(EventType.ExecuteReaderAsync));

        Assert.Equal(request.SpanId, sql.ParentSpanId);
        Assert.Equal(3, sql.GetTagItem("db.response.rows_read"));
    }

    private static async Task ReadAllAsync(ProfileDbConnection con)
    {
#pragma warning disable CA2007
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT id, val FROM t ORDER BY id";

        await using var reader = await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
#pragma warning restore CA2007
        while (await reader.ReadAsync(TestContext.Current.CancellationToken).ConfigureAwait(true))
        {
        }
    }

    [Fact]
    public void ReaderNotWrappedEmitsSingleActivity()
    {
        // Arrange
        using var recorder = new ActivityRecorder(SourceName);
        using var listener = new OpenTelemetryListener(new OpenTelemetryListenerOption());
        using var con = CreateConnection(listener, wrapReader: false);

        recorder.Activities.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
            }
        }

        // Assert
        var activity = Assert.Single(recorder.Activities);
        Assert.Equal(nameof(EventType.ExecuteReader), activity.DisplayName);
        Assert.Null(activity.GetTagItem("db.response.rows_read"));
    }

    [Fact]
    public void NestedCommandWhileReaderOpenEmitsSeparateActivities()
    {
        // Arrange
        using var recorder = new ActivityRecorder(SourceName);
        using var listener = new OpenTelemetryListener(new OpenTelemetryListenerOption());
        using var con = CreateConnection(listener, wrapReader: true);

        recorder.Activities.Clear();

        // Act
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                using var nested = con.CreateCommand();
                nested.CommandText = "SELECT COUNT(*) FROM t";
                nested.ExecuteScalar();
            }
        }

        // Assert
        Assert.Equal(4, recorder.Activities.Count);
        Assert.Equal(3, recorder.Activities.Count(x => x.DisplayName == nameof(EventType.ExecuteScalar)));

        var readerActivity = Assert.Single(recorder.Activities, x => x.DisplayName == nameof(EventType.ExecuteReader));
        Assert.Equal(3, readerActivity.GetTagItem("db.response.rows_read"));
    }

    [Fact]
    public void ReaderWrappedNestsSingleChildUnderRequestActivity()
    {
        // Arrange
        using var requestSource = new ActivitySource(RequestSourceName);
        using var recorder = new ActivityRecorder(SourceName, RequestSourceName);
        using var listener = new OpenTelemetryListener(new OpenTelemetryListenerOption());
        using var con = CreateConnection(listener, wrapReader: true);

        recorder.Activities.Clear();

        // Act
        using (requestSource.StartActivity("GET /api/items", ActivityKind.Server))
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT id, val FROM t ORDER BY id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
            }
        }

        // Assert
        Assert.Equal(2, recorder.Activities.Count);

        var request = Assert.Single(recorder.Activities, x => x.DisplayName == "GET /api/items");
        var sql = Assert.Single(recorder.Activities, x => x.DisplayName == nameof(EventType.ExecuteReader));

        Assert.Equal(request.SpanId, sql.ParentSpanId);
        Assert.Equal(3, sql.GetTagItem("db.response.rows_read"));
    }

    [Fact]
    public void ReaderFailedEmitsSingleErrorActivity()
    {
        // Arrange
        using var recorder = new ActivityRecorder(SourceName);
        using var listener = new OpenTelemetryListener(new OpenTelemetryListenerOption());
        using var con = CreateConnection(listener, wrapReader: true);

        recorder.Activities.Clear();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "INVALID SQL";

        // Act & Assert
        Assert.Throws<SqliteException>(cmd.ExecuteReader);

        var activity = Assert.Single(recorder.Activities);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }
}
