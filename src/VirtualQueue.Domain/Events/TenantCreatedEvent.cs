using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record TenantCreatedEvent(Guid TenantId, string Name, string Domain) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
