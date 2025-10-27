using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserLoggedInEvent(
    Guid UserId,
    Guid TenantId,
    string Username,
    DateTime LoginTime) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
