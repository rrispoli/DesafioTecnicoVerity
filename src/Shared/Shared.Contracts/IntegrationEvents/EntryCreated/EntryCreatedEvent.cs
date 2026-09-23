namespace Shared.Contracts.IntegrationEvents.EntryCreated;

public sealed record EntryCreatedEvent(
    Guid EntryId,
    EntryCreatedType Type,
    decimal Amount,
    DateTimeOffset OccurredAt);
