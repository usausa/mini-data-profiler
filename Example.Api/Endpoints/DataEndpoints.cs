namespace Example.Api.Endpoints;

using Example.Api.Data;
using Example.Api.Models;

using Microsoft.AspNetCore.Http.HttpResults;

public static class DataEndpoints
{
    public static IEndpointRouteBuilder MapDataEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/items").WithTags("Items");

        // ExecuteReader (async) + ReaderFinished. Optional ?type= filter adds a parameter
        group.MapGet("/", async (DataRepository repository, string? type, CancellationToken cancellationToken) =>
                TypedResults.Ok(await repository.QueryAllAsync(type, cancellationToken)))
            .WithSummary("List items")
            .WithDescription("Async ExecuteReader. Pass ?type=A to add a parameterized filter.");

        // ExecuteReader (synchronous)
        group.MapGet("/sync", (DataRepository repository) =>
                TypedResults.Ok(repository.QueryAllSync()))
            .WithSummary("List items (sync)")
            .WithDescription("Synchronous ExecuteReader, tagged with the ExecuteReader event.");

        // ExecuteScalar
        group.MapGet("/count", async (DataRepository repository, CancellationToken cancellationToken) =>
                TypedResults.Ok(new CountResponse(await repository.CountAsync(cancellationToken))))
            .WithSummary("Count items")
            .WithDescription("ExecuteScalar (SELECT COUNT(*)).");

        // DbDataSource command path (ProfileDbDataSourceCommand)
        group.MapGet("/via-datasource", async (DataRepository repository, CancellationToken cancellationToken) =>
                TypedResults.Ok(await repository.QueryViaDataSourceCommandAsync(cancellationToken)))
            .WithSummary("List items via DbDataSource")
            .WithDescription("Executes through ProfileDbDataSource.CreateCommand (ProfileDbDataSourceCommand).");

        // CommandFailed (invalid SQL)
        group.MapGet("/error", async (DataRepository repository, CancellationToken cancellationToken) =>
            {
                await repository.QueryInvalidAsync(cancellationToken);
                return Results.NoContent();
            })
            .WithSummary("Trigger a failure")
            .WithDescription("Runs invalid SQL to demonstrate the CommandFailed event and an error span / 500 response.");

        // ExecuteReader (single row) -> 404 when missing
        group.MapGet("/{id:long}", async Task<Results<Ok<DataEntity>, NotFound>> (DataRepository repository, long id, CancellationToken cancellationToken) =>
            {
                var entity = await repository.FindAsync(id, cancellationToken);
                return entity is null ? TypedResults.NotFound() : TypedResults.Ok(entity);
            })
            .WithSummary("Get an item by id")
            .WithDescription("Async ExecuteReader returning a single row.");

        // ExecuteNonQuery (insert)
        group.MapPost("/", async (DataRepository repository, CreateItemRequest request, CancellationToken cancellationToken) =>
                TypedResults.Ok(new AffectedResponse(await repository.InsertAsync(request, cancellationToken))))
            .WithSummary("Insert an item")
            .WithDescription("ExecuteNonQuery (INSERT). Id is assigned by SQLite.");

        // ExecuteNonQuery (update)
        group.MapPut("/{id:long}", async Task<Results<Ok<AffectedResponse>, NotFound>> (DataRepository repository, long id, UpdateItemRequest request, CancellationToken cancellationToken) =>
            {
                var affected = await repository.UpdateAsync(id, request, cancellationToken);
                return affected == 0 ? TypedResults.NotFound() : TypedResults.Ok(new AffectedResponse(affected));
            })
            .WithSummary("Update an item")
            .WithDescription("ExecuteNonQuery (UPDATE).");

        // ExecuteNonQuery (delete)
        group.MapDelete("/{id:long}", async Task<Results<Ok<AffectedResponse>, NotFound>> (DataRepository repository, long id, CancellationToken cancellationToken) =>
            {
                var affected = await repository.DeleteAsync(id, cancellationToken);
                return affected == 0 ? TypedResults.NotFound() : TypedResults.Ok(new AffectedResponse(affected));
            })
            .WithSummary("Delete an item")
            .WithDescription("ExecuteNonQuery (DELETE).");

        // Transaction (commit or rollback)
        group.MapPost("/transaction", async (DataRepository repository, bool? commit, CancellationToken cancellationToken) =>
            {
                var shouldCommit = commit ?? true;
                var affected = await repository.TransactionAsync(shouldCommit, cancellationToken);
                return TypedResults.Ok(new TransactionResponse(shouldCommit, affected));
            })
            .WithSummary("Insert inside a transaction")
            .WithDescription("Begins a transaction, inserts a row, then commits (?commit=true, default) or rolls back (?commit=false).");

        return app;
    }
}
