# Example.Api

A small ASP.NET Core Web API (SQLite + raw ADO.NET) wired up with MiniDataProfiler.

## Endpoints and the profiler events they exercise

| Endpoint | ADO.NET call | MiniDataProfiler events |
|---|---|---|
| `GET /api/items` | `ExecuteReaderAsync` | `ReaderExecuting/Executed`, `CommandFinally`, `ReaderFinished` |
| `GET /api/items/sync` | `ExecuteReader` | `ReaderExecuting/Executed`, `CommandFinally`, `ReaderFinished` |
| `GET /api/items/count` | `ExecuteScalarAsync` | `ScalarExecuting/Executed`, `CommandFinally` |
| `GET /api/items/{id}` | `ExecuteReaderAsync` | `ReaderExecuting/Executed`, `CommandFinally`, `ReaderFinished` |
| `GET /api/items/via-datasource` | `DbDataSource` command | same as reader, via `ProfileDbDataSourceCommand` |
| `POST /api/items` | `ExecuteNonQueryAsync` | `NonQueryExecuting/Executed`, `CommandFinally` |
| `PUT /api/items/{id}` | `ExecuteNonQueryAsync` | `NonQueryExecuting/Executed`, `CommandFinally` |
| `DELETE /api/items/{id}` | `ExecuteNonQueryAsync` | `NonQueryExecuting/Executed`, `CommandFinally` |
| `POST /api/items/transaction?commit=` | `BeginTransaction` + `ExecuteNonQueryAsync` + commit/rollback | `NonQueryExecuting/Executed`, `CommandFinally` |
| `GET /api/items/error` | invalid SQL | `CommandFailed`, `CommandFinally` (error span / 500) |
