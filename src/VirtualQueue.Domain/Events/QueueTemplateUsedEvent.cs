using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueTemplateUsedEvent(
    Guid TemplateId,
    Guid TenantId,
    string TemplateName,
    int UsageCount) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

