using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.Common.Interfaces;

public interface IQueueRepository : IRepository<Queue>
{
    Task<IEnumerable<Queue>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Queue?> GetByTenantIdAndIdAsync(Guid tenantId, Guid queueId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Queue>> GetActiveQueuesAsync(CancellationToken cancellationToken = default);
}
