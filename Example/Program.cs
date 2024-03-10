using System.Diagnostics;

using Example.Accessor;
using Example.Models;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using MiniDataProfiler;
using MiniDataProfiler.Exporter.Logging;
using MiniDataProfiler.Exporter.OpenTelemetry;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

// Setup Logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("MiniDataProfiler.Exporter.Logging", LogLevel.Information)
        .AddConsole();
});
var logExporter = new LoggingExporter(loggerFactory.CreateLogger<LoggingExporter>(), new LoggingExporterOption());

// Setup OpenTelemetry
using var exampleSource = new ActivitySource("Example");
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .ConfigureResource(config =>
    {
        config.AddService("Example", serviceInstanceId: Environment.MachineName);
    })
    .AddSource("Example")
    .AddMiniDataProfilerInstrumentation()
    .AddOtlpExporter(config =>
    {
        config.Endpoint = new Uri("http://otlp-exporter:4317");
    })
    .Build();

// Exporters
var exporter = new ChainExporter(logExporter, new OpenTelemetryExporter(new OpenTelemetryExporterOption()));

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
// ReSharper disable once ExplicitCallerInfoArgument
using var activity = exampleSource.StartActivity("Operation");

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
