using VirtualQueue.Domain.Common;
using BCrypt.Net;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents an API key in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity manages API keys for tenant authentication
/// and access control to the system.
/// </remarks>
public class ApiKey : BaseEntity
{
    #region Constants
    private const int MaxKeyLength = 100;
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MaxPermissionsLength = 2000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this API key.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the name of the API key.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the description of the API key.
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Gets the actual API key value.
    /// </summary>
    public string Key { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the hashed version of the API key.
    /// </summary>
    public string KeyHash { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the permissions associated with this API key.
    /// </summary>
    public string? Permissions { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this API key is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Gets the date and time when this API key expires.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }
    
    /// <summary>
    /// Gets the date and time of the last usage.
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }
    
    /// <summary>
    /// Gets the number of times this API key has been used.
    /// </summary>
    public long UsageCount { get; private set; } = 0;
    
    /// <summary>
    /// Gets the IP address restrictions for this API key.
    /// </summary>
    public string? IpRestrictions { get; private set; }
    
    /// <summary>
    /// Gets the rate limit for this API key (requests per minute).
    /// </summary>
    public int RateLimit { get; private set; } = 1000;
    
    /// <summary>
    /// Gets the user identifier who created this API key.
    /// </summary>
    public string? CreatedBy { get; private set; }
    #endregion

    #region Constructors
    private ApiKey() { } // EF Core constructor

    public ApiKey(
        Guid tenantId,
        string name,
        string key,
        string? description = null,
        string? permissions = null,
        DateTime? expiresAt = null,
        string? ipRestrictions = null,
        int rateLimit = 1000,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
        
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        if (key.Length > MaxKeyLength)
            throw new ArgumentException($"Key cannot exceed {MaxKeyLength} characters", nameof(key));
        
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        
        if (!string.IsNullOrEmpty(permissions) && permissions.Length > MaxPermissionsLength)
            throw new ArgumentException($"Permissions cannot exceed {MaxPermissionsLength} characters", nameof(permissions));
        
        if (rateLimit < 0)
            throw new ArgumentException("Rate limit cannot be negative", nameof(rateLimit));

        TenantId = tenantId;
        Name = name;
        Key = key;
        KeyHash = BCrypt.Net.BCrypt.HashPassword(key);
        Description = description;
        Permissions = permissions;
        ExpiresAt = expiresAt;
        IpRestrictions = ipRestrictions;
        RateLimit = rateLimit;
        CreatedBy = createdBy;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the API key name.
    /// </summary>
    /// <param name="name">The new name.</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
        
        Name = name;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the API key description.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void UpdateDescription(string? description)
    {
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        
        Description = description;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the API key permissions.
    /// </summary>
    /// <param name="permissions">The new permissions.</param>
    public void UpdatePermissions(string? permissions)
    {
        if (!string.IsNullOrEmpty(permissions) && permissions.Length > MaxPermissionsLength)
            throw new ArgumentException($"Permissions cannot exceed {MaxPermissionsLength} characters", nameof(permissions));
        
        Permissions = permissions;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the API key expiration date.
    /// </summary>
    /// <param name="expiresAt">The new expiration date.</param>
    public void UpdateExpiration(DateTime? expiresAt)
    {
        ExpiresAt = expiresAt;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the API key rate limit.
    /// </summary>
    /// <param name="rateLimit">The new rate limit.</param>
    public void UpdateRateLimit(int rateLimit)
    {
        if (rateLimit < 0)
            throw new ArgumentException("Rate limit cannot be negative", nameof(rateLimit));
        
        RateLimit = rateLimit;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the API key.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the API key.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records usage of the API key.
    /// </summary>
    public void RecordUsage()
    {
        UsageCount++;
        LastUsedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if the API key is expired.
    /// </summary>
    /// <returns>True if the API key is expired, false otherwise.</returns>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the provided key matches this API key.
    /// </summary>
    /// <param name="key">The key to verify.</param>
    /// <returns>True if the key matches, false otherwise.</returns>
    public bool VerifyKey(string key)
    {
        return BCrypt.Net.BCrypt.Verify(key, KeyHash);
    }
    #endregion
}
