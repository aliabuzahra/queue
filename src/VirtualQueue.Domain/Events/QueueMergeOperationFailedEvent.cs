using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Events;

public record QueueMergeOperationFailedEvent(
    Guid OperationId,
    Guid TenantId,
    Guid SourceQueueId,
    Guid DestinationQueueId,
    string ErrorMessage) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

