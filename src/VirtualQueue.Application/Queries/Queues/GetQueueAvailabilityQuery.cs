using MediatR;

namespace VirtualQueue.Application.Queries.Queues;

public record GetQueueAvailabilityQuery(
    Guid TenantId,
    Guid QueueId,
    DateTime? CheckTime = null
) : IRequest<QueueAvailabilityDto>;

public record QueueAvailabilityDto(
    bool IsAvailable,
    DateTime? NextActivationTime,
    DateTime? PreviousActivationTime,
    string? Reason
);
