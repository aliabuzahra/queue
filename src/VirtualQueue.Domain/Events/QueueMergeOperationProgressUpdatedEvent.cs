using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueMergeOperationProgressUpdatedEvent(
    Guid OperationId,
    Guid TenantId,
    int UsersMoved,
    int TotalUsers) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

