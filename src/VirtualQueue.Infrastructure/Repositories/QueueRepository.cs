using Microsoft.EntityFrameworkCore;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Infrastructure.Data;

namespace VirtualQueue.Infrastructure.Repositories;

public class QueueRepository : BaseRepository<Queue>, IQueueRepository
{
    public QueueRepository(VirtualQueueDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Queue>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(q => q.TenantId == tenantId).ToListAsync(cancellationToken);
    }

    public async Task<Queue?> GetByTenantIdAndIdAsync(Guid tenantId, Guid queueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(q => q.TenantId == tenantId && q.Id == queueId, cancellationToken);
    }

    public async Task<IEnumerable<Queue>> GetActiveQueuesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(q => q.IsActive).ToListAsync(cancellationToken);
    }
}
