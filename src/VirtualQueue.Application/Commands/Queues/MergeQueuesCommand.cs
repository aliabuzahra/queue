using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public record MergeQueuesCommand(
    Guid TenantId,
    Guid SourceQueueId,
    Guid DestinationQueueId,
    int? MaxUsersToMove = null) : IRequest<QueueMergeOperationResultDto>;

