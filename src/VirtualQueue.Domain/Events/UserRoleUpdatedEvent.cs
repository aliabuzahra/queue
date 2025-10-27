using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserRoleUpdatedEvent(
    Guid UserId,
    Guid TenantId,
    string Username,
    string OldRole,
    string NewRole) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
