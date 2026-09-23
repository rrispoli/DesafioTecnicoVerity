namespace Entries.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(string type, string payload, CancellationToken cancellationToken = default);
}
