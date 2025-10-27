using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueMergeOperationCompletedEvent(
    Guid OperationId,
    Guid TenantId,
    Guid SourceQueueId,
    Guid DestinationQueueId,
    int UsersMoved) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

