using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Common.Interfaces;

public interface IQueueMergeService
{
    // Merge Operations
    Task<QueueMergeOperationDto> CreateMergeOperationAsync(Guid tenantId, CreateQueueMergeOperationRequest request, CancellationToken cancellationToken = default);
    Task<QueueMergeOperationDto?> GetMergeOperationAsync(Guid operationId, CancellationToken cancellationToken = default);
    Task<List<QueueMergeOperationDto>> GetMergeOperationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<QueueMergeOperationProgressDto> GetMergeOperationProgressAsync(Guid operationId, CancellationToken cancellationToken = default);
    
    // Merge Execution
    Task<QueueMergeOperationResultDto> ExecuteMergeOperationAsync(Guid operationId, CancellationToken cancellationToken = default);
    Task<bool> CancelMergeOperationAsync(Guid operationId, CancellationToken cancellationToken = default);
    Task<bool> PauseMergeOperationAsync(Guid operationId, CancellationToken cancellationToken = default);
    Task<bool> ResumeMergeOperationAsync(Guid operationId, CancellationToken cancellationToken = default);
    
    // Queue Operations
    Task<QueueMergeOperationResultDto> MergeQueuesAsync(Guid tenantId, Guid sourceQueueId, Guid destinationQueueId, int? maxUsersToMove = null, CancellationToken cancellationToken = default);
    Task<QueueMergeOperationResultDto> SplitQueueAsync(Guid tenantId, Guid sourceQueueId, int usersToMove, string newQueueName, CancellationToken cancellationToken = default);
    Task<QueueMergeOperationResultDto> RebalanceQueuesAsync(Guid tenantId, List<Guid> queueIds, CancellationToken cancellationToken = default);
    
    // Validation
    Task<bool> ValidateMergeOperationAsync(Guid tenantId, Guid sourceQueueId, Guid destinationQueueId, CancellationToken cancellationToken = default);
    Task<List<string>> GetMergeOperationWarningsAsync(Guid tenantId, Guid sourceQueueId, Guid destinationQueueId, CancellationToken cancellationToken = default);
}

