using MediatR;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Application.Commands.UserSessions;

public class ReleaseUsersCommandHandler : IRequestHandler<ReleaseUsersCommand, int>
{
    private readonly IQueueRepository _queueRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ICacheService _cacheService;

    public ReleaseUsersCommandHandler(
        IQueueRepository queueRepository,
        IUserSessionRepository userSessionRepository,
        ICacheService cacheService)
    {
        _queueRepository = queueRepository;
        _userSessionRepository = userSessionRepository;
        _cacheService = cacheService;
    }

    public async Task<int> Handle(ReleaseUsersCommand request, CancellationToken cancellationToken)
    {
        // Verify queue exists
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        if (queue == null)
        {
            throw new InvalidOperationException($"Queue with ID '{request.QueueId}' not found for tenant");
        }

        if (!queue.IsActive)
        {
            throw new InvalidOperationException("Cannot release users from inactive queue");
        }

        // Get waiting users
        var waitingUsers = await _userSessionRepository.GetWaitingUsersByQueueIdAsync(request.QueueId, cancellationToken);
        var usersToRelease = waitingUsers
            .OrderBy(u => u.EnqueuedAt)
            .Take(request.Count)
            .ToList();

        var releasedCount = 0;
        foreach (var user in usersToRelease)
        {
            user.MarkAsReleased();
            releasedCount++;
            
            // Remove from cache
            await _cacheService.RemoveUserPositionAsync(request.QueueId, user.UserIdentifier, cancellationToken);
        }

        if (releasedCount > 0)
        {
            await _userSessionRepository.SaveChangesAsync(cancellationToken);
        }

        return releasedCount;
    }
}
