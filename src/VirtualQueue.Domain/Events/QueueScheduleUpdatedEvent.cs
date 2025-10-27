using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public class QueueScheduleUpdatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid QueueId { get; }
    public Guid TenantId { get; }

    public QueueScheduleUpdatedEvent(Guid queueId, Guid tenantId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        QueueId = queueId;
        TenantId = tenantId;
    }
}
