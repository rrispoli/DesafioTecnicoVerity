using Entries.Domain.Enums;
using Shared.Domain.Common;

namespace Entries.Domain.Entities;

public class Entry : Entity
{
    public EntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string Description { get; private set; }

    public Entry(EntryType type, decimal amount, DateTimeOffset occurredAt, string description)
    {
        Type = type;
        Amount = amount;
        OccurredAt = occurredAt;
        Description = description;
    }
}
