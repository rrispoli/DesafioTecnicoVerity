using Shared.Domain.Common;

namespace DailyBalance.Domain.Entities;

public class ProcessedEvent : Entity
{
    public Guid EventId { get; private set; }

    public ProcessedEvent(Guid eventId)
    {
        EventId = eventId;
    }
}
