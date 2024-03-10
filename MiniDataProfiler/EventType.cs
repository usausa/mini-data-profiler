namespace MiniDataProfiler;

public enum EventType
{
    None,
    ExecuteNonQuery,
    ExecuteNonQueryAsync,
    ExecuteScalar,
    ExecuteScalarAsync,
    ExecuteReader,
    ExecuteReaderAsync,
}

public static class EventTypeExtensions
{
    public static string AsString(this EventType eventType) =>
        eventType switch
        {
            EventType.ExecuteNonQuery => nameof(EventType.ExecuteNonQuery),
            EventType.ExecuteNonQueryAsync => nameof(EventType.ExecuteNonQueryAsync),
            EventType.ExecuteScalar => nameof(EventType.ExecuteScalar),
            EventType.ExecuteScalarAsync => nameof(EventType.ExecuteScalarAsync),
            EventType.ExecuteReader => nameof(EventType.ExecuteReader),
            EventType.ExecuteReaderAsync => nameof(EventType.ExecuteReaderAsync),
            _ => string.Empty
        };
}
