using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class AuditLoggingService : IAuditLoggingService
{
    private readonly ILogger<AuditLoggingService> _logger;
    private readonly ICacheService _cacheService;

    public AuditLoggingService(ILogger<AuditLoggingService> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task LogActionAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            // Log to structured logging
            _logger.LogInformation(
                "Audit: {Action} on {EntityType} {EntityId} by {UserIdentifier} in tenant {TenantId}",
                entry.Action,
                entry.EntityType,
                entry.EntityId,
                entry.UserIdentifier,
                entry.TenantId);

            // Store in cache for quick access (in production, you'd also store in database)
            var cacheKey = $"audit:{entry.TenantId}:{entry.Id}";
            await _cacheService.SetAsync(cacheKey, entry, TimeSpan.FromDays(30), cancellationToken);

            // Add to audit log list for the tenant
            var auditLogKey = $"audit_logs:{entry.TenantId}";
            var existingLogs = await _cacheService.GetAsync<List<AuditLogEntry>>(auditLogKey, cancellationToken) ?? new List<AuditLogEntry>();
            existingLogs.Add(entry);
            
            // Keep only last 1000 entries per tenant
            if (existingLogs.Count > 1000)
            {
                existingLogs = existingLogs.OrderByDescending(x => x.Timestamp).Take(1000).ToList();
            }
            
            await _cacheService.SetAsync(auditLogKey, existingLogs, TimeSpan.FromDays(30), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry for action {Action}", entry.Action);
        }
    }

    public async Task<List<AuditLogEntry>> GetAuditLogsAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null, string? action = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogKey = $"audit_logs:{tenantId}";
            var logs = await _cacheService.GetAsync<List<AuditLogEntry>>(auditLogKey, cancellationToken) ?? new List<AuditLogEntry>();

            var filteredLogs = logs.AsQueryable();

            if (startDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(action))
            {
                filteredLogs = filteredLogs.Where(x => x.Action == action);
            }

            return filteredLogs.OrderByDescending(x => x.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs for tenant {TenantId}", tenantId);
            return new List<AuditLogEntry>();
        }
    }

    public async Task<List<AuditLogEntry>> GetUserAuditLogsAsync(Guid tenantId, string userIdentifier, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogKey = $"audit_logs:{tenantId}";
            var logs = await _cacheService.GetAsync<List<AuditLogEntry>>(auditLogKey, cancellationToken) ?? new List<AuditLogEntry>();

            var filteredLogs = logs.Where(x => x.UserIdentifier == userIdentifier);

            if (startDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= endDate.Value);
            }

            return filteredLogs.OrderByDescending(x => x.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user audit logs for tenant {TenantId}, user {UserIdentifier}", tenantId, userIdentifier);
            return new List<AuditLogEntry>();
        }
    }

    public async Task<List<AuditLogEntry>> GetQueueAuditLogsAsync(Guid tenantId, Guid queueId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogKey = $"audit_logs:{tenantId}";
            var logs = await _cacheService.GetAsync<List<AuditLogEntry>>(auditLogKey, cancellationToken) ?? new List<AuditLogEntry>();

            var filteredLogs = logs.Where(x => x.EntityType == "Queue" && x.EntityId == queueId.ToString());

            if (startDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredLogs = filteredLogs.Where(x => x.Timestamp <= endDate.Value);
            }

            return filteredLogs.OrderByDescending(x => x.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue audit logs for tenant {TenantId}, queue {QueueId}", tenantId, queueId);
            return new List<AuditLogEntry>();
        }
    }

    public async Task CleanupOldLogsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would clean up old logs from the database
            // For now, we'll just log the cleanup action
            _logger.LogInformation("Cleaning up audit logs older than {CutoffDate}", cutoffDate);
            
            // This would typically involve:
            // 1. Querying the database for old logs
            // 2. Archiving them to cold storage
            // 3. Deleting them from the main database
            // 4. Updating cache accordingly
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old audit logs");
        }
    }

    // New methods required by the interface
    public async Task LogActionAsync(string action, string entityType, string entityId, string userIdentifier, string? userIpAddress = null, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEntry = new AuditLogEntry(
                Guid.NewGuid(),
                tenantId ?? Guid.Empty,
                action,
                entityType,
                entityId,
                userIdentifier,
                userIpAddress,
                null,
                null,
                DateTime.UtcNow,
                null
            );

            await LogActionAsync(auditEntry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit action {Action} for {EntityType} {EntityId}", action, entityType, entityId);
        }
    }

    public async Task<AuditLogEntry?> GetAuditLogEntryAsync(Guid logId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would query the database by log ID
            // For now, we'll search through cached logs
            var cacheKey = $"audit_logs:*";
            // This is a simplified implementation - in reality you'd need to search across all tenants
            // or maintain a separate index by log ID
            
            _logger.LogInformation("Retrieving audit log entry {LogId}", logId);
            
            // Return null for now - in a real implementation this would query the database
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit log entry {LogId}", logId);
            return null;
        }
    }
}
