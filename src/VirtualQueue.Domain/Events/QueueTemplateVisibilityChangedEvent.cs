using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueTemplateVisibilityChangedEvent(
    Guid TemplateId,
    Guid TenantId,
    string TemplateName,
    bool IsPublic) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

