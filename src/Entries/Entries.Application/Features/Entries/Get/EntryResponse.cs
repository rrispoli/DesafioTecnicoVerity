using Entries.Domain.Entities;
using Entries.Domain.Enums;

namespace Entries.Application.Features.Entries.Get;

public record EntryResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public EntryType Type { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public string Description { get; init; }

    public EntryResponse(Entry entity)
    {
        Id = entity.Id;
        CreatedAt = entity.CreatedAt;
        Type = entity.Type;
        Amount = entity.Amount;
        OccurredAt = entity.OccurredAt;
        Description = entity.Description;
    }
}
