using MediatR;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Application.Queries.Queues;

public class GetQueueAvailabilityQueryHandler : IRequestHandler<GetQueueAvailabilityQuery, QueueAvailabilityDto>
{
    private readonly IQueueRepository _queueRepository;

    public GetQueueAvailabilityQueryHandler(IQueueRepository queueRepository)
    {
        _queueRepository = queueRepository;
    }

    public async Task<QueueAvailabilityDto> Handle(GetQueueAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        if (queue == null)
        {
            return new QueueAvailabilityDto(false, null, null, "Queue not found");
        }

        var checkTime = request.CheckTime ?? DateTime.UtcNow;
        var isAvailable = queue.IsQueueAvailable(checkTime);
        
        string? reason = null;
        if (!isAvailable)
        {
            if (!queue.IsActive)
                reason = "Queue is inactive";
            else if (queue.Schedule != null)
                reason = "Queue is outside scheduled hours";
        }

        var nextActivation = queue.GetNextActivationTime(checkTime);
        var previousActivation = queue.GetPreviousActivationTime(checkTime);

        return new QueueAvailabilityDto(isAvailable, nextActivation, previousActivation, reason);
    }
}
