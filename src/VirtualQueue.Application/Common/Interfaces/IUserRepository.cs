using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(Guid tenantId, string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameOrEmailAsync(Guid tenantId, string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<List<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<User>> GetByRoleAsync(Guid tenantId, string role, CancellationToken cancellationToken = default);
    Task<List<User>> GetByStatusAsync(Guid tenantId, string status, CancellationToken cancellationToken = default);
    Task<List<User>> SearchUsersAsync(Guid tenantId, string searchTerm, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(Guid tenantId, string username, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
    Task<int> GetUserCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<User>> GetRecentlyActiveUsersAsync(Guid tenantId, DateTime since, CancellationToken cancellationToken = default);
}
