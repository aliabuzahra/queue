using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.Queries.UserSessions;

public class GetUserQueueStatusQueryHandler : IRequestHandler<GetUserQueueStatusQuery, QueueStatusDto?>
{
    private readonly IQueueRepository _queueRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetUserQueueStatusQueryHandler(
        IQueueRepository queueRepository,
        IUserSessionRepository userSessionRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _queueRepository = queueRepository;
        _userSessionRepository = userSessionRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<QueueStatusDto?> Handle(GetUserQueueStatusQuery request, CancellationToken cancellationToken)
    {
        // Verify queue exists
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        if (queue == null)
        {
            return null;
        }

        // Get user session
        var userSession = await _userSessionRepository.GetByQueueIdAndUserIdentifierAsync(
            request.QueueId, request.UserIdentifier, cancellationToken);
        
        if (userSession == null)
        {
            return null;
        }

        // Get position from cache or calculate
        var position = await _cacheService.GetUserPositionAsync(request.QueueId, request.UserIdentifier, cancellationToken);
        if (position == null)
        {
            position = await _userSessionRepository.GetUserPositionInQueueAsync(
                request.QueueId, request.UserIdentifier, cancellationToken);
            await _cacheService.SetUserPositionAsync(request.QueueId, request.UserIdentifier, position.Value, cancellationToken);
        }

        // Get queue statistics
        var waitingCount = await _userSessionRepository.GetWaitingUsersCountByQueueIdAsync(request.QueueId, cancellationToken);
        var servingCount = queue.GetServingUsersCount();
        var releasedCount = queue.Users.Count(u => u.Status == QueueStatus.Released);

        return new QueueStatusDto(
            request.QueueId,
            request.UserIdentifier,
            position.Value,
            waitingCount,
            servingCount,
            releasedCount,
            userSession.EnqueuedAt,
            userSession.Status.ToString()
        );
    }
}
