using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.Common.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
}
