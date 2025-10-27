using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserReleasedEvent(Guid UserSessionId, Guid QueueId, string UserIdentifier) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
