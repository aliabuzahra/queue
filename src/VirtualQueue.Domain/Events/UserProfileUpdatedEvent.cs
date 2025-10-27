using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserProfileUpdatedEvent(
    Guid UserId,
    Guid TenantId,
    string Username) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
