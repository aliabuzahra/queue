using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents an audit log entry in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity provides comprehensive audit trail capabilities
/// for tracking all system activities and changes.
/// </remarks>
public class AuditLog : BaseEntity
{
    #region Constants
    private const int MaxActionLength = 100;
    private const int MaxEntityTypeLength = 50;
    private const int MaxOldValuesLength = 5000;
    private const int MaxNewValuesLength = 5000;
    private const int MaxIpAddressLength = 45;
    private const int MaxUserAgentLength = 500;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this audit log entry.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the user identifier who performed the action.
    /// </summary>
    public string UserIdentifier { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the action that was performed.
    /// </summary>
    public string Action { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the type of entity that was affected.
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the identifier of the entity that was affected.
    /// </summary>
    public Guid EntityId { get; private set; }
    
    /// <summary>
    /// Gets the old values before the change.
    /// </summary>
    public string? OldValues { get; private set; }
    
    /// <summary>
    /// Gets the new values after the change.
    /// </summary>
    public string? NewValues { get; private set; }
    
    /// <summary>
    /// Gets the timestamp when the action was performed.
    /// </summary>
    public DateTime ActionTimestamp { get; private set; }
    
    /// <summary>
    /// Gets the IP address from which the action was performed.
    /// </summary>
    public string? IpAddress { get; private set; }
    
    /// <summary>
    /// Gets the user agent string from the client.
    /// </summary>
    public string? UserAgent { get; private set; }
    
    /// <summary>
    /// Gets additional metadata about the action.
    /// </summary>
    public string? Metadata { get; private set; }
    
    /// <summary>
    /// Gets the result of the action (Success, Failure, etc.).
    /// </summary>
    public AuditResult Result { get; private set; } = AuditResult.Success;
    
    /// <summary>
    /// Gets the error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }
    #endregion

    #region Constructors
    private AuditLog() { } // EF Core constructor

    public AuditLog(
        Guid tenantId,
        string userIdentifier,
        string action,
        string entityType,
        Guid entityId,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? metadata = null,
        AuditResult result = AuditResult.Success,
        string? errorMessage = null)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));
        
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));
        
        if (action.Length > MaxActionLength)
            throw new ArgumentException($"Action cannot exceed {MaxActionLength} characters", nameof(action));
        
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));
        
        if (entityType.Length > MaxEntityTypeLength)
            throw new ArgumentException($"Entity type cannot exceed {MaxEntityTypeLength} characters", nameof(entityType));
        
        if (!string.IsNullOrEmpty(oldValues) && oldValues.Length > MaxOldValuesLength)
            throw new ArgumentException($"Old values cannot exceed {MaxOldValuesLength} characters", nameof(oldValues));
        
        if (!string.IsNullOrEmpty(newValues) && newValues.Length > MaxNewValuesLength)
            throw new ArgumentException($"New values cannot exceed {MaxNewValuesLength} characters", nameof(newValues));
        
        if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Length > MaxIpAddressLength)
            throw new ArgumentException($"IP address cannot exceed {MaxIpAddressLength} characters", nameof(ipAddress));
        
        if (!string.IsNullOrEmpty(userAgent) && userAgent.Length > MaxUserAgentLength)
            throw new ArgumentException($"User agent cannot exceed {MaxUserAgentLength} characters", nameof(userAgent));

        TenantId = tenantId;
        UserIdentifier = userIdentifier;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Metadata = metadata;
        Result = result;
        ErrorMessage = errorMessage;
        ActionTimestamp = DateTime.UtcNow;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the audit log with additional metadata.
    /// </summary>
    /// <param name="metadata">The additional metadata.</param>
    public void UpdateMetadata(string? metadata)
    {
        Metadata = metadata;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the audit log result.
    /// </summary>
    /// <param name="result">The result of the action.</param>
    /// <param name="errorMessage">The error message if applicable.</param>
    public void UpdateResult(AuditResult result, string? errorMessage = null)
    {
        Result = result;
        ErrorMessage = errorMessage;
        MarkAsUpdated();
    }
    #endregion
}

/// <summary>
/// Represents the result of an audited action.
/// </summary>
public enum AuditResult
{
    Success,
    Failure,
    PartialSuccess,
    Cancelled,
    Timeout
}
