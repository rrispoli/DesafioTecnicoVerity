using Entries.Application.Abstractions;
using Entries.Domain.Entities;
using Entries.Domain.Enums;
using Shared.Application.Abstractions.Messaging;
using Shared.Contracts.IntegrationEvents.EntryCreated;
using Shared.Domain.Common;
using System.Text.Json;

namespace Entries.Application.Features.Entries.Create;

public class CreateEntryCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<CreateEntryCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateEntryCommand command, CancellationToken cancellationToken = default)
    {
        var entry = new Entry(command.Type, command.Amount, command.OccurredAt, command.Description);
        dbContext.Entries.Add(entry);

        var entryCreatedType = entry.Type == EntryType.Credit ? EntryCreatedType.Credit : EntryCreatedType.Debit;
        var entryCreatedEvent = new EntryCreatedEvent(entry.Id, entryCreatedType, entry.Amount, entry.OccurredAt);
        var outboxMessage = new OutboxMessage(nameof(EntryCreatedEvent), JsonSerializer.Serialize(entryCreatedEvent));
        dbContext.OutboxMessages.Add(outboxMessage);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(entry.Id);
    }
}
