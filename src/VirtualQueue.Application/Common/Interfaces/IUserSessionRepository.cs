using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.Common.Interfaces;

public interface IUserSessionRepository : IRepository<UserSession>
{
    Task<IEnumerable<UserSession>> GetByQueueIdAsync(Guid queueId, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByQueueIdAndUserIdentifierAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetByStatusAsync(QueueStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetWaitingUsersByQueueIdAsync(Guid queueId, CancellationToken cancellationToken = default);
    Task<int> GetWaitingUsersCountByQueueIdAsync(Guid queueId, CancellationToken cancellationToken = default);
    Task<int> GetUserPositionInQueueAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.UserSession>> GetByQueueIdAndDateRangeAsync(Guid queueId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<int> GetUserCountByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<int> GetReleasedUserCountByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
