using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserSuspendedEvent(
    Guid UserId,
    Guid TenantId,
    string Username,
    string Reason) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
