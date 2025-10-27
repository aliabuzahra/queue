using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueMergeOperationCreatedEvent(
    Guid OperationId,
    Guid TenantId,
    Guid SourceQueueId,
    Guid DestinationQueueId,
    string OperationType) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

