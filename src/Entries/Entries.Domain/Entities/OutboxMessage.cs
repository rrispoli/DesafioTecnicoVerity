using Shared.Domain.Common;

namespace Entries.Domain.Entities;

public class OutboxMessage : Entity
{
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }

    public OutboxMessage(string type, string payload)
    {
        Type = type;
        Payload = payload;
    }

    public void MarkAsProcessed() => ProcessedAt = UpdatedAt = DateTimeOffset.UtcNow;

    public void RegisterFailure(string error)
    {
        RetryCount++;
        Error = error;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
