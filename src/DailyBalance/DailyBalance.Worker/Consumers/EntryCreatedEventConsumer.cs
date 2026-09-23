using DailyBalance.Application.Features.Balances.Update;
using MassTransit;
using Shared.Application.Abstractions.Messaging;
using Shared.Contracts.IntegrationEvents.EntryCreated;

namespace DailyBalance.Worker.Consumers;

public class EntryCreatedEventConsumer(ICommandHandler<UpdateBalanceCommand, Guid> handler) : IConsumer<EntryCreatedEvent>
{
    public async Task Consume(ConsumeContext<EntryCreatedEvent> context)
    {
        var command = new UpdateBalanceCommand(
            context.Message.EntryId,
            context.Message.Type,
            context.Message.Amount,
            context.Message.OccurredAt);

        await handler.HandleAsync(command, context.CancellationToken);
    }
}
