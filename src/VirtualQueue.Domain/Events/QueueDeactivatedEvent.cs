using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueDeactivatedEvent(Guid QueueId, Guid TenantId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
