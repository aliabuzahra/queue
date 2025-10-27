using Microsoft.EntityFrameworkCore;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Infrastructure.Data;

namespace VirtualQueue.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly VirtualQueueDbContext _context;

    public UserRepository(VirtualQueueDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(Guid tenantId, string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(Guid tenantId, string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && 
                (u.Username == usernameOrEmail || u.Email == usernameOrEmail), cancellationToken);
    }

    public async Task<List<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetByRoleAsync(Guid tenantId, string role, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse(role, true, out Domain.Enums.UserRole userRole))
            return new List<User>();

        return await _context.Users
            .Where(u => u.TenantId == tenantId && u.Role == userRole)
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetByStatusAsync(Guid tenantId, string status, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse(status, true, out Domain.Enums.UserStatus userStatus))
            return new List<User>();

        return await _context.Users
            .Where(u => u.TenantId == tenantId && u.Status == userStatus)
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> SearchUsersAsync(Guid tenantId, string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();
        
        return await _context.Users
            .Where(u => u.TenantId == tenantId && 
                (u.Username.ToLower().Contains(term) || 
                 u.Email.ToLower().Contains(term) || 
                 u.FirstName.ToLower().Contains(term) || 
                 u.LastName.ToLower().Contains(term)))
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(Guid tenantId, string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.TenantId == tenantId && u.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.TenantId == tenantId && u.Email == email, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUserCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .CountAsync(u => u.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<User>> GetRecentlyActiveUsersAsync(Guid tenantId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= since)
            .OrderByDescending(u => u.LastLoginAt)
            .ToListAsync(cancellationToken);
    }
}
