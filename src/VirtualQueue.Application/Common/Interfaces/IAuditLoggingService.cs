namespace VirtualQueue.Application.Common.Interfaces;

public interface IAuditLoggingService
{
    // Methods expected by controllers
    Task LogActionAsync(string action, string entityType, string entityId, string userIdentifier, string? userIpAddress = null, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<AuditLogEntry?> GetAuditLogEntryAsync(Guid logId, CancellationToken cancellationToken = default);
    
    // Original methods
    Task LogActionAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
    Task<List<AuditLogEntry>> GetAuditLogsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null, string? action = null, CancellationToken cancellationToken = default);
    Task<List<AuditLogEntry>> GetUserAuditLogsAsync(Guid tenantId, string userIdentifier, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<AuditLogEntry>> GetQueueAuditLogsAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task CleanupOldLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}

public record AuditLogEntry(
    Guid Id,
    Guid TenantId,
    string Action,
    string EntityType,
    string EntityId,
    string UserIdentifier,
    string? UserIpAddress,
    string? UserAgent,
    Dictionary<string, object>? Metadata,
    DateTime Timestamp,
    string? Description = null
);

public enum AuditAction
{
    // Tenant actions
    TenantCreated,
    TenantUpdated,
    TenantDeleted,
    
    // Queue actions
    QueueCreated,
    QueueUpdated,
    QueueDeleted,
    QueueActivated,
    QueueDeactivated,
    QueueScheduled,
    
    // User session actions
    UserEnqueued,
    UserReleased,
    UserDropped,
    UserServed,
    UserPositionUpdated,
    
    // System actions
    SystemStartup,
    SystemShutdown,
    ConfigurationChanged,
    SecurityEvent,
    ErrorOccurred
}
