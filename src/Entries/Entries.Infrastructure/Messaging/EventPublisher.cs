using Entries.Application.Abstractions;
using MassTransit;
using Shared.Contracts.IntegrationEvents.EntryCreated;
using System.Text.Json;

namespace Entries.Infrastructure.Messaging;

public class EventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishAsync(string type, string payload, CancellationToken cancellationToken = default)
    {
        var integrationEvent = type switch
        {
            nameof(EntryCreatedEvent) => JsonSerializer.Deserialize<EntryCreatedEvent>(payload)!,
            _ => throw new InvalidOperationException($"Tipo de evento desconhecido: {type}")
        };

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}