namespace Shared.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }
}
