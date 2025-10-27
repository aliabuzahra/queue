using Microsoft.EntityFrameworkCore;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;
using VirtualQueue.Infrastructure.Data;

namespace VirtualQueue.Infrastructure.Repositories;

public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(VirtualQueueDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserSession>> GetByQueueIdAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(us => us.QueueId == queueId).ToListAsync(cancellationToken);
    }

    public async Task<UserSession?> GetByQueueIdAndUserIdentifierAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(us => us.QueueId == queueId && us.UserIdentifier == userIdentifier, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetByStatusAsync(QueueStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(us => us.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetWaitingUsersByQueueIdAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(us => us.QueueId == queueId && us.Status == QueueStatus.Waiting)
            .OrderBy(us => us.EnqueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetWaitingUsersCountByQueueIdAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(us => us.QueueId == queueId && us.Status == QueueStatus.Waiting, cancellationToken);
    }

    public async Task<int> GetUserPositionInQueueAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default)
    {
        var userSession = await GetByQueueIdAndUserIdentifierAsync(queueId, userIdentifier, cancellationToken);
        if (userSession == null)
            return -1;

        var waitingUsersBefore = await _dbSet.CountAsync(
            us => us.QueueId == queueId && 
                  us.Status == QueueStatus.Waiting && 
                  us.EnqueuedAt < userSession.EnqueuedAt, 
            cancellationToken);

        return waitingUsersBefore + 1;
    }

    public async Task<List<Domain.Entities.UserSession>> GetByQueueIdAndDateRangeAsync(Guid queueId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.QueueId == queueId && u.EnqueuedAt >= startDate && u.EnqueuedAt <= endDate)
            .OrderBy(u => u.EnqueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUserCountByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.EnqueuedAt >= startDate && u.EnqueuedAt <= endDate)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetReleasedUserCountByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.Status == Domain.Enums.QueueStatus.Released && 
                       u.ReleasedAt >= startDate && u.ReleasedAt <= endDate)
            .CountAsync(cancellationToken);
    }
}
