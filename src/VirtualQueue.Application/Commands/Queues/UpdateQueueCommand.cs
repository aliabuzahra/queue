using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public record UpdateQueueCommand(
    Guid TenantId,
    Guid QueueId,
    string Name,
    string Description,
    int MaxConcurrentUsers,
    int ReleaseRatePerMinute
) : IRequest<QueueDto>;
