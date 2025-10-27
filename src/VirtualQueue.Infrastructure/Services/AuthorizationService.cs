using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VirtualQueue.Infrastructure.Services;

/// <summary>
/// Production-ready authorization service with database-backed permissions.
/// </summary>
/// <remarks>
/// This service provides comprehensive authorization capabilities including
/// role-based access control, permission checking, and tenant isolation.
/// </remarks>
public class AuthorizationService : IAuthorizationService
{
    #region Fields
    private readonly VirtualQueueDbContext _context;
    private readonly ILogger<AuthorizationService> _logger;
    private readonly ICacheService _cacheService;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="cacheService">The cache service instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required parameters are null.
    /// </exception>
    public AuthorizationService(
        VirtualQueueDbContext context,
        ILogger<AuthorizationService> logger,
        ICacheService cacheService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Checks if a user has a specific permission in a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the permission, false otherwise.</returns>
    public async Task<bool> HasPermissionAsync(Guid tenantId, string userIdentifier, string permission, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));
            
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("Permission cannot be null or empty", nameof(permission));

            _logger.LogDebug("Checking permission {Permission} for user {UserIdentifier} in tenant {TenantId}", 
                permission, userIdentifier, tenantId);

            // Check cache first
            var cacheKey = $"permission:{tenantId}:{userIdentifier}:{permission}";
            var cachedResult = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogDebug("Permission check result from cache: {Result}", cachedResult);
                return bool.Parse(cachedResult);
            }

            // Check database
            var hasPermission = await CheckPermissionInDatabaseAsync(tenantId, userIdentifier, permission, cancellationToken);

            // Cache the result for 5 minutes
            await _cacheService.SetAsync(cacheKey, hasPermission.ToString(), TimeSpan.FromMinutes(5), cancellationToken);

            _logger.LogDebug("Permission check result: {Result}", hasPermission);
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserIdentifier}", permission, userIdentifier);
            return false;
        }
    }

    /// <summary>
    /// Checks if a user has a specific role in a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    public async Task<bool> HasRoleAsync(Guid tenantId, string userIdentifier, string role, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));
            
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null or empty", nameof(role));

            _logger.LogDebug("Checking role {Role} for user {UserIdentifier} in tenant {TenantId}", 
                role, userIdentifier, tenantId);

            // Check cache first
            var cacheKey = $"role:{tenantId}:{userIdentifier}:{role}";
            var cachedResult = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogDebug("Role check result from cache: {Result}", cachedResult);
                return bool.Parse(cachedResult);
            }

            // Check database
            var hasRole = await CheckRoleInDatabaseAsync(tenantId, userIdentifier, role, cancellationToken);

            // Cache the result for 5 minutes
            await _cacheService.SetAsync(cacheKey, hasRole.ToString(), TimeSpan.FromMinutes(5), cancellationToken);

            _logger.LogDebug("Role check result: {Result}", hasRole);
            return hasRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role {Role} for user {UserIdentifier}", role, userIdentifier);
            return false;
        }
    }

    /// <summary>
    /// Gets all permissions for a user in a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of permissions.</returns>
    public async Task<List<string>> GetUserPermissionsAsync(Guid tenantId, string userIdentifier, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));

            _logger.LogDebug("Getting permissions for user {UserIdentifier} in tenant {TenantId}", userIdentifier, tenantId);

            // Check cache first
            var cacheKey = $"user_permissions:{tenantId}:{userIdentifier}";
            var cachedPermissions = await _cacheService.GetAsync<List<string>>(cacheKey, cancellationToken);
            if (cachedPermissions != null)
            {
                _logger.LogDebug("Permissions retrieved from cache: {Count} permissions", cachedPermissions.Count);
                return cachedPermissions;
            }

            // Get from database
            var permissions = await GetUserPermissionsFromDatabaseAsync(tenantId, userIdentifier, cancellationToken);

            // Cache the result for 5 minutes
            await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(5), cancellationToken);

            _logger.LogDebug("Permissions retrieved from database: {Count} permissions", permissions.Count);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserIdentifier}", userIdentifier);
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets all roles for a user in a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of roles.</returns>
    public async Task<List<string>> GetUserRolesAsync(Guid tenantId, string userIdentifier, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));

            _logger.LogDebug("Getting roles for user {UserIdentifier} in tenant {TenantId}", userIdentifier, tenantId);

            // Check cache first
            var cacheKey = $"user_roles:{tenantId}:{userIdentifier}";
            var cachedRoles = await _cacheService.GetAsync<List<string>>(cacheKey, cancellationToken);
            if (cachedRoles != null)
            {
                _logger.LogDebug("Roles retrieved from cache: {Count} roles", cachedRoles.Count);
                return cachedRoles;
            }

            // Get from database
            var roles = await GetUserRolesFromDatabaseAsync(tenantId, userIdentifier, cancellationToken);

            // Cache the result for 5 minutes
            await _cacheService.SetAsync(cacheKey, roles, TimeSpan.FromMinutes(5), cancellationToken);

            _logger.LogDebug("Roles retrieved from database: {Count} roles", roles.Count);
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserIdentifier}", userIdentifier);
            return new List<string>();
        }
    }

    /// <summary>
    /// Authorizes a user for a specific resource and action.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="resource">The resource being accessed.</param>
    /// <param name="action">The action being performed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if authorized, false otherwise.</returns>
    public async Task<bool> AuthorizeAsync(Guid tenantId, string userIdentifier, string resource, string action, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));
            
            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be null or empty", nameof(resource));
            
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));

            _logger.LogDebug("Authorizing user {UserIdentifier} for {Action} on {Resource} in tenant {TenantId}", 
                userIdentifier, action, resource, tenantId);

            // Check cache first
            var cacheKey = $"authorize:{tenantId}:{userIdentifier}:{resource}:{action}";
            var cachedResult = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogDebug("Authorization result from cache: {Result}", cachedResult);
                return bool.Parse(cachedResult);
            }

            // Check if user has permission for the specific resource and action
            var permission = $"{resource}.{action}";
            var isAuthorized = await HasPermissionAsync(tenantId, userIdentifier, permission, cancellationToken);

            // Cache the result for 5 minutes
            await _cacheService.SetAsync(cacheKey, isAuthorized.ToString(), TimeSpan.FromMinutes(5), cancellationToken);

            _logger.LogDebug("Authorization result: {Result}", isAuthorized);
            return isAuthorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authorizing user {UserIdentifier} for {Action} on {Resource}", userIdentifier, action, resource);
            return false;
        }
    }

    /// <summary>
    /// Gets all permissions for a specific role.
    /// </summary>
    /// <param name="role">The role name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of permissions for the role.</returns>
    public async Task<List<string>> GetPermissionsForRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null or empty", nameof(role));

            _logger.LogDebug("Getting permissions for role {Role}", role);

            // Check cache first
            var cacheKey = $"role_permissions:{role}";
            var cachedPermissions = await _cacheService.GetAsync<List<string>>(cacheKey, cancellationToken);
            if (cachedPermissions != null)
            {
                _logger.LogDebug("Role permissions retrieved from cache: {Count} permissions", cachedPermissions.Count);
                return cachedPermissions;
            }

            // Get permissions for the role
            var permissions = GetRolePermissionsFromEnum(role);

            // Cache the result for 30 minutes (role permissions don't change often)
            await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(30), cancellationToken);

            _logger.LogDebug("Role permissions retrieved: {Count} permissions", permissions.Count);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role {Role}", role);
            return new List<string>();
        }
    }

    /// <summary>
    /// Invalidates the authorization cache for a user.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task InvalidateUserCacheAsync(Guid tenantId, string userIdentifier, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return;

            _logger.LogDebug("Invalidating authorization cache for user {UserIdentifier} in tenant {TenantId}", userIdentifier, tenantId);

            // Remove all cached authorization data for this user
            var patterns = new[]
            {
                $"permission:{tenantId}:{userIdentifier}:*",
                $"role:{tenantId}:{userIdentifier}:*",
                $"user_permissions:{tenantId}:{userIdentifier}",
                $"user_roles:{tenantId}:{userIdentifier}",
                $"authorize:{tenantId}:{userIdentifier}:*"
            };

            foreach (var pattern in patterns)
            {
                await _cacheService.RemoveAsync(pattern, cancellationToken);
            }

            _logger.LogDebug("Authorization cache invalidated for user {UserIdentifier}", userIdentifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for user {UserIdentifier}", userIdentifier);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Checks permission in the database.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the permission, false otherwise.</returns>
    private async Task<bool> CheckPermissionInDatabaseAsync(Guid tenantId, string userIdentifier, string permission, CancellationToken cancellationToken)
    {
        try
        {
            // Get user from database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == userIdentifier, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserIdentifier} not found in tenant {TenantId}", userIdentifier, tenantId);
                return false;
            }

            // Check if user is active
            if (user.Status != Domain.Enums.UserStatus.Active)
            {
                _logger.LogWarning("User {UserIdentifier} is not active", userIdentifier);
                return false;
            }

            // Get role-based permissions
            var rolePermissions = GetRolePermissions(user.Role);
            
            // Check if permission is granted by role
            if (rolePermissions.Contains(permission))
            {
                return true;
            }

            // Check API key permissions if user has API access
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(ak => ak.TenantId == tenantId && ak.CreatedBy == userIdentifier, cancellationToken);

            if (apiKey != null && apiKey.IsActive && !apiKey.IsExpired())
            {
                var apiPermissions = ParsePermissions(apiKey.Permissions);
                if (apiPermissions.Contains(permission))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission in database for user {UserIdentifier}", userIdentifier);
            return false;
        }
    }

    /// <summary>
    /// Checks role in the database.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="role">The role to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    private async Task<bool> CheckRoleInDatabaseAsync(Guid tenantId, string userIdentifier, string role, CancellationToken cancellationToken)
    {
        try
        {
            // Get user from database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == userIdentifier, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserIdentifier} not found in tenant {TenantId}", userIdentifier, tenantId);
                return false;
            }

            // Check if user is active
            if (user.Status != Domain.Enums.UserStatus.Active)
            {
                _logger.LogWarning("User {UserIdentifier} is not active", userIdentifier);
                return false;
            }

            // Check if user has the specified role
            return user.Role.ToString().Equals(role, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role in database for user {UserIdentifier}", userIdentifier);
            return false;
        }
    }

    /// <summary>
    /// Gets user permissions from the database.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of permissions.</returns>
    private async Task<List<string>> GetUserPermissionsFromDatabaseAsync(Guid tenantId, string userIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            // Get user from database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == userIdentifier, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserIdentifier} not found in tenant {TenantId}", userIdentifier, tenantId);
                return new List<string>();
            }

            // Check if user is active
            if (user.Status != Domain.Enums.UserStatus.Active)
            {
                _logger.LogWarning("User {UserIdentifier} is not active", userIdentifier);
                return new List<string>();
            }

            var permissions = new List<string>();

            // Get role-based permissions
            var rolePermissions = GetRolePermissions(user.Role);
            permissions.AddRange(rolePermissions);

            // Get API key permissions if user has API access
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(ak => ak.TenantId == tenantId && ak.CreatedBy == userIdentifier, cancellationToken);

            if (apiKey != null && apiKey.IsActive && !apiKey.IsExpired())
            {
                var apiPermissions = ParsePermissions(apiKey.Permissions);
                permissions.AddRange(apiPermissions);
            }

            return permissions.Distinct().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions from database for user {UserIdentifier}", userIdentifier);
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets user roles from the database.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of roles.</returns>
    private async Task<List<string>> GetUserRolesFromDatabaseAsync(Guid tenantId, string userIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            // Get user from database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == userIdentifier, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserIdentifier} not found in tenant {TenantId}", userIdentifier, tenantId);
                return new List<string>();
            }

            // Check if user is active
            if (user.Status != Domain.Enums.UserStatus.Active)
            {
                _logger.LogWarning("User {UserIdentifier} is not active", userIdentifier);
                return new List<string>();
            }

            var roles = new List<string> { user.Role.ToString() };

            // Add API key role if user has API access
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(ak => ak.TenantId == tenantId && ak.CreatedBy == userIdentifier, cancellationToken);

            if (apiKey != null && apiKey.IsActive && !apiKey.IsExpired())
            {
                roles.Add("ApiUser");
            }

            return roles.Distinct().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles from database for user {UserIdentifier}", userIdentifier);
            return new List<string>();
        }
    }

    /// <summary>
    /// Gets permissions for a specific role.
    /// </summary>
    /// <param name="role">The user role.</param>
    /// <returns>List of permissions.</returns>
    private static List<string> GetRolePermissions(Domain.Enums.UserRole role)
    {
        return role switch
        {
            Domain.Enums.UserRole.Admin => new List<string>
            {
                "queue.create", "queue.read", "queue.update", "queue.delete",
                "user.create", "user.read", "user.update", "user.delete",
                "tenant.read", "tenant.update",
                "analytics.read", "reports.read",
                "system.admin", "system.config"
            },
            Domain.Enums.UserRole.Manager => new List<string>
            {
                "queue.create", "queue.read", "queue.update",
                "user.read", "user.update",
                "analytics.read", "reports.read"
            },
            Domain.Enums.UserRole.User => new List<string>
            {
                "queue.read", "queue.update",
                "user.read"
            },
            Domain.Enums.UserRole.Guest => new List<string>
            {
                "queue.join", "queue.read"
            },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Gets permissions for a specific role by role name.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>List of permissions.</returns>
    private static List<string> GetRolePermissionsFromEnum(string roleName)
    {
        if (Enum.TryParse<Domain.Enums.UserRole>(roleName, true, out var role))
        {
            return GetRolePermissions(role);
        }
        
        // Handle special roles
        return roleName.ToLowerInvariant() switch
        {
            "apiuser" => new List<string>
            {
                "queue.read", "queue.update",
                "user.read"
            },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Parses permissions from a JSON string.
    /// </summary>
    /// <param name="permissionsJson">The permissions JSON string.</param>
    /// <returns>List of permissions.</returns>
    private static List<string> ParsePermissions(string? permissionsJson)
    {
        if (string.IsNullOrWhiteSpace(permissionsJson))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(permissionsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
    #endregion
}