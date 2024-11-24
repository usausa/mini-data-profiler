# mini-data-profiler

| Package | Info | Description |
|-|-|-|
| MiniDataProfiler | [![NuGet](https://img.shields.io/nuget/v/MiniDataProfiler.svg)](https://www.nuget.org/packages/MiniDataProfiler/) | Core |
| MiniDataProfiler.Listener.Logging | [![NuGet](https://img.shields.io/nuget/v/MiniDataProfiler.Listener.Logging.svg)](https://www.nuget.org/packages/MiniDataProfiler.Listener.Logging/) | Microsoft.Extensions.Logging Listener |
| MiniDataProfiler.Listener.OpenTelemetry | [![NuGet](https://img.shields.io/nuget/v/MiniDataProfiler.Listener.OpenTelemetry.svg)](https://www.nuget.org/packages/MiniDataProfiler.Listener.OpenTelemetry/) | OpenTelemetry Listener |

## What is this?

* Simple profiler for ADO.NET

<img src="Images/trace.png" title="trace">

## Usage

```csharp
// Setup Logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("MiniDataProfiler.Listener.Logging", LogLevel.Information)
        .AddConsole();
});
var logListener = new LoggingListener(loggerFactory.CreateLogger<LoggingListener>(), new LoggingListenerOption());

// Setup OpenTelemetry
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .ConfigureResource(config =>
    {
        config.AddService("Example", serviceInstanceId: Environment.MachineName);
    })
    .AddMiniDataProfilerInstrumentation()
    .AddOtlpExporter(config =>
    {
        config.Endpoint = new Uri("http://otlp-exporter:4317");
    })
    .Build();

// Listeners
var listener = new ChainListener(logListener, new OpenTelemetryListener(new OpenTelemetryListenerOption()));

// Use ProfileDbConnection
using var con = new ProfileDbConnection(listener, new SqliteConnection(connectionString));
...
```
