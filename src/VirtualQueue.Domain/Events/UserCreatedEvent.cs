using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserCreatedEvent(
    Guid UserId,
    Guid TenantId,
    string Username,
    string Email) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
