using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record UserPhoneVerifiedEvent(
    Guid UserId,
    Guid TenantId,
    string Username,
    string PhoneNumber) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
