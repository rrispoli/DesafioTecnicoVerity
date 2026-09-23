using Shared.Application.Abstractions.Messaging;
using Shared.Contracts.IntegrationEvents.EntryCreated;

namespace DailyBalance.Application.Features.Balances.Update;

public record UpdateBalanceCommand(
    Guid EntryId,
    EntryCreatedType Type,
    decimal Amount,
    DateTimeOffset OccurredAt)
    : ICommand<Guid>;
