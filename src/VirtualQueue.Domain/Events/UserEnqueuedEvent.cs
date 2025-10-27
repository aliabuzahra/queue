using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserEnqueuedEvent(Guid UserSessionId, Guid QueueId, Guid TenantId, string UserIdentifier) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
