using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueCreatedEvent(Guid QueueId, Guid TenantId, string Name) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
