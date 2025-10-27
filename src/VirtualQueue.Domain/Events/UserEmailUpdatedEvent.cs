using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserEmailUpdatedEvent(
    Guid UserId,
    Guid TenantId,
    string Username,
    string OldEmail,
    string NewEmail) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
