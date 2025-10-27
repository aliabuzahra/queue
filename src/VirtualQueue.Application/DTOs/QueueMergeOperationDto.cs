namespace VirtualQueue.Application.DTOs;

public record QueueMergeOperationDto(
    Guid Id,
    Guid TenantId,
    Guid SourceQueueId,
    Guid DestinationQueueId,
    string OperationType,
    string Status,
    int UsersToMove,
    int UsersMoved,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    Dictionary<string, string> Metadata,
    DateTime CreatedAt);

public record CreateQueueMergeOperationRequest(
    Guid SourceQueueId,
    Guid DestinationQueueId,
    string OperationType,
    int UsersToMove);

public record QueueMergeOperationProgressDto(
    Guid OperationId,
    int UsersMoved,
    int TotalUsers,
    double ProgressPercentage,
    string Status,
    TimeSpan? Duration,
    string? ErrorMessage);

public record QueueMergeOperationResultDto(
    bool Success,
    int UsersMoved,
    string Message,
    TimeSpan Duration);

