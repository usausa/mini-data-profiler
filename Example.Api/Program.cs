using Example.Api.Data;
using Example.Api.Endpoints;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using MiniDataProfiler;
using MiniDataProfiler.Listener.Logging;
using MiniDataProfiler.Listener.OpenTelemetry;

using NSwag.AspNetCore;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var databasePath = Path.Combine(AppContext.BaseDirectory, "example.db");
var connectionString = $"Data Source={databasePath}";

// ---------------------------------------------------------------------------
// OpenTelemetry
// ---------------------------------------------------------------------------
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        // MiniDataProfiler.Listener.OpenTelemetry: one span per SQL operation.
        .AddMiniDataProfilerInstrumentation());

if (!String.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
    builder.Services.AddOpenTelemetry().UseOtlpExporter();
}

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

// ---------------------------------------------------------------------------
// API services
// ---------------------------------------------------------------------------
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// ---------------------------------------------------------------------------
// MiniDataProfiler wiring
// ---------------------------------------------------------------------------

// OpenTelemetry listener owns an ActivitySource
builder.Services.AddSingleton(_ => new OpenTelemetryListener(new OpenTelemetryListenerOption()));

// Chain the logging and OpenTelemetry listeners
builder.Services.AddSingleton<IProfileListener>(provider =>
{
    var loggingListener = new LoggingListener(
        provider.GetRequiredService<ILogger<LoggingListener>>(),
        new LoggingListenerOption
        {
            OutputStartLog = true,
            OutputFinallyLog = true,
            OutputExceptionLog = true,
            OutputParameter = true
        });

    return new ChainListener(loggingListener, provider.GetRequiredService<OpenTelemetryListener>());
});

// Wrap a SQLite DbDataSource with the profiler
builder.Services.AddSingleton(provider => new ProfileDbDataSource(
    provider.GetRequiredService<IProfileListener>(),
    new SqliteDbDataSource(connectionString),
    new ProfilerOption { WrapDataReader = true }));

builder.Services.AddScoped<DataRepository>();

// ---------------------------------------------------------------------------
// Build
// ---------------------------------------------------------------------------
var app = builder.Build();

// Create sample database
await DatabaseInitializer.InitializeAsync(connectionString);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.UseSwaggerUi(settings => settings.SwaggerRoutes.Add(new SwaggerUiRoute("v1", "/openapi/v1.json")));

app.MapDataEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = registration => registration.Tags.Contains("live") });
}

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

await app.RunAsync();
