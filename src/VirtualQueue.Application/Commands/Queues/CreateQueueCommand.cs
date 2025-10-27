using MediatR;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Application.Commands.Queues;

public record CreateQueueCommand(
    Guid TenantId,
    string Name,
    string Description,
    int MaxConcurrentUsers,
    int ReleaseRatePerMinute
) : IRequest<QueueDto>;
