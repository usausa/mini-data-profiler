using Example.Accessor;
using Example.Models;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using MiniDataProfiler;
using MiniDataProfiler.Exporter.Logging;

using Smart.Data;
using Smart.Data.Accessor;
using Smart.Data.Accessor.Engine;

// Initialize database
const string fileName = "test.db";
const string connectionString = "Data Source=" + fileName;

if (File.Exists(fileName))
{
    File.Delete(fileName);
}

// Create exporter
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("MiniDataProfiler.Exporter.Logging", LogLevel.Information)
        .AddConsole();
});
var exporter = new LoggingExporter(loggerFactory.CreateLogger<LoggingExporter>(), new LoggingExporterOption());

// Create accessor
var engine = new ExecuteEngineConfig()
    .ConfigureComponents(c =>
    {
        c.Add<IDbProvider>(new DelegateDbProvider(() => new ProfileDbConnection(exporter, new SqliteConnection(connectionString))));
    })
    .ToEngine();
var factory = new DataAccessorFactory(engine);

var accessor = factory.Create<IExampleAccessor>();

// Operation
accessor.Create();

accessor.Insert(new DataEntity { Id = 1L, Name = "Data-1", Type = "A" });
accessor.Insert(new DataEntity { Id = 2L, Name = "Data-2", Type = "B" });
accessor.Insert(new DataEntity { Id = 3L, Name = "Data-3", Type = "A" });

var typeA = accessor.QueryDataList("A");
Console.WriteLine(typeA.Count);

var all = accessor.QueryDataList();
Console.WriteLine(all.Count);

var ordered = accessor.QueryDataList(order: "Name DESC");
Console.WriteLine(ordered[0].Name);
