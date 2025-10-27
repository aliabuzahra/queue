using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueTemplateUpdatedEvent(
    Guid TemplateId,
    Guid TenantId,
    string TemplateName) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
