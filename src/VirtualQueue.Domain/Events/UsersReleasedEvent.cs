using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UsersReleasedEvent(Guid QueueId, Guid TenantId, int ReleasedCount) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
