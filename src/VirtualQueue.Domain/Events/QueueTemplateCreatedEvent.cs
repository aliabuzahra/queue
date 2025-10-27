using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueTemplateCreatedEvent(
    Guid TemplateId,
    Guid TenantId,
    string TemplateName,
    string TemplateType) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
