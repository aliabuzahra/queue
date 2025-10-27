using Microsoft.EntityFrameworkCore;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Infrastructure.Data;

namespace VirtualQueue.Infrastructure.Repositories;

public class TenantRepository : BaseRepository<Tenant>, ITenantRepository
{
    public TenantRepository(VirtualQueueDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Domain == domain, cancellationToken);
    }

    public async Task<Tenant?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.ApiKey == apiKey, cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(t => t.IsActive).ToListAsync(cancellationToken);
    }
}
