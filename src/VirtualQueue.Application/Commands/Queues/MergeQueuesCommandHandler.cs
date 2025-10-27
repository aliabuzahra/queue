using MediatR;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public class MergeQueuesCommandHandler : IRequestHandler<MergeQueuesCommand, QueueMergeOperationResultDto>
{
    private readonly IQueueMergeService _queueMergeService;
    private readonly ILogger<MergeQueuesCommandHandler> _logger;

    public MergeQueuesCommandHandler(
        IQueueMergeService queueMergeService,
        ILogger<MergeQueuesCommandHandler> logger)
    {
        _queueMergeService = queueMergeService;
        _logger = logger;
    }

    public async Task<QueueMergeOperationResultDto> Handle(MergeQueuesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Merging queues {SourceQueueId} into {DestinationQueueId} for tenant {TenantId}", 
            request.SourceQueueId, request.DestinationQueueId, request.TenantId);

        // Validate merge operation
        var isValid = await _queueMergeService.ValidateMergeOperationAsync(
            request.TenantId, 
            request.SourceQueueId, 
            request.DestinationQueueId, 
            cancellationToken);

        if (!isValid)
        {
            _logger.LogWarning("Merge operation validation failed for queues {SourceQueueId} -> {DestinationQueueId}", 
                request.SourceQueueId, request.DestinationQueueId);
            return new QueueMergeOperationResultDto(false, 0, "Merge operation validation failed", TimeSpan.Zero);
        }

        // Check for warnings
        var warnings = await _queueMergeService.GetMergeOperationWarningsAsync(
            request.TenantId, 
            request.SourceQueueId, 
            request.DestinationQueueId, 
            cancellationToken);

        if (warnings.Any())
        {
            _logger.LogWarning("Merge operation has warnings: {Warnings}", string.Join(", ", warnings));
        }

        // Execute merge operation
        var result = await _queueMergeService.MergeQueuesAsync(
            request.TenantId,
            request.SourceQueueId,
            request.DestinationQueueId,
            request.MaxUsersToMove,
            cancellationToken);

        _logger.LogInformation("Queue merge completed: {Success}, {UsersMoved} users moved in {Duration}", 
            result.Success, result.UsersMoved, result.Duration);

        return result;
    }
}

