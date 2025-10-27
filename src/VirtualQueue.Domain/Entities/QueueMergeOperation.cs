using VirtualQueue.Domain.Common;
using VirtualQueue.Domain.Events;

namespace VirtualQueue.Domain.Entities;

public class QueueMergeOperation : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SourceQueueId { get; private set; }
    public Guid DestinationQueueId { get; private set; }
    public string OperationType { get; private set; }
    public string Status { get; private set; }
    public int UsersToMove { get; private set; }
    public int UsersMoved { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }

    private QueueMergeOperation() { } // EF Core constructor

    public QueueMergeOperation(
        Guid tenantId,
        Guid sourceQueueId,
        Guid destinationQueueId,
        string operationType,
        int usersToMove)
    {
        if (string.IsNullOrWhiteSpace(operationType))
            throw new ArgumentException("Operation type cannot be null or empty", nameof(operationType));

        TenantId = tenantId;
        SourceQueueId = sourceQueueId;
        DestinationQueueId = destinationQueueId;
        OperationType = operationType;
        Status = "Pending";
        UsersToMove = usersToMove;
        UsersMoved = 0;
        Metadata = new Dictionary<string, string>();

        AddDomainEvent(new QueueMergeOperationCreatedEvent(Id, TenantId, SourceQueueId, DestinationQueueId, OperationType));
    }

    public void Start()
    {
        if (Status != "Pending")
            throw new InvalidOperationException("Operation can only be started from Pending status");

        Status = "InProgress";
        StartedAt = DateTime.UtcNow;

        AddDomainEvent(new QueueMergeOperationStartedEvent(Id, TenantId, SourceQueueId, DestinationQueueId));
    }

    public void UpdateProgress(int usersMoved)
    {
        if (Status != "InProgress")
            throw new InvalidOperationException("Operation must be in progress to update");

        UsersMoved = usersMoved;
        AddDomainEvent(new QueueMergeOperationProgressUpdatedEvent(Id, TenantId, UsersMoved, UsersToMove));
    }

    public void Complete()
    {
        if (Status != "InProgress")
            throw new InvalidOperationException("Operation must be in progress to complete");

        Status = "Completed";
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new QueueMergeOperationCompletedEvent(Id, TenantId, SourceQueueId, DestinationQueueId, UsersMoved));
    }

    public void Fail(string errorMessage)
    {
        if (Status == "Completed")
            throw new InvalidOperationException("Cannot fail a completed operation");

        Status = "Failed";
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;

        AddDomainEvent(new QueueMergeOperationFailedEvent(Id, TenantId, SourceQueueId, DestinationQueueId, errorMessage));
    }

    public void Cancel()
    {
        if (Status == "Completed")
            throw new InvalidOperationException("Cannot cancel a completed operation");

        Status = "Cancelled";
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new QueueMergeOperationCancelledEvent(Id, TenantId, SourceQueueId, DestinationQueueId));
    }

    public void UpdateMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));

        Metadata[key] = value;
    }

    public TimeSpan? GetDuration()
    {
        if (StartedAt == null) return null;
        var endTime = CompletedAt ?? DateTime.UtcNow;
        return endTime - StartedAt.Value;
    }

    public double GetProgressPercentage()
    {
        if (UsersToMove == 0) return 100.0;
        return (double)UsersMoved / UsersToMove * 100.0;
    }
}

