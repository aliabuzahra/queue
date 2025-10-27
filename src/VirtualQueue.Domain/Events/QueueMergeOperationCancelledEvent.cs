using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueMergeOperationCancelledEvent(
    Guid OperationId,
    Guid TenantId,
    Guid SourceQueueId,
    Guid DestinationQueueId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

