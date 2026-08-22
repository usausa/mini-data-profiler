namespace Example.Api.Models;

public sealed record TransactionResponse(bool Committed, int Affected);
