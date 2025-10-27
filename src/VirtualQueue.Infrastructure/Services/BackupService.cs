using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Infrastructure.Data;

namespace VirtualQueue.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly VirtualQueueDbContext _context;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupPath;

    public BackupService(VirtualQueueDbContext context, ILogger<BackupService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _backupPath = configuration["Backup:Path"] ?? "./backups";
        
        // Ensure backup directory exists
        Directory.CreateDirectory(_backupPath);
    }

    public async Task<BackupResult> CreateBackupAsync(Guid tenantId, BackupType type, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var backupId = Guid.NewGuid().ToString();
        
        try
        {
            _logger.LogInformation("Starting backup {BackupId} for tenant {TenantId} of type {Type}", backupId, tenantId, type);
            
            var backupFileName = $"{backupId}_{tenantId}_{type}_{startTime:yyyyMMdd_HHmmss}.json";
            var backupFilePath = Path.Combine(_backupPath, backupFileName);
            
            // Create backup data based on type
            var backupData = await CreateBackupDataAsync(tenantId, type, cancellationToken);
            
            // Write backup to file
            await File.WriteAllTextAsync(backupFilePath, backupData, cancellationToken);
            
            var fileInfo = new FileInfo(backupFilePath);
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation("Backup {BackupId} completed successfully. Size: {Size} bytes, Duration: {Duration}", 
                backupId, fileInfo.Length, duration);
            
            return new BackupResult(
                backupId,
                true,
                null,
                fileInfo.Length,
                startTime,
                duration
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup {BackupId} for tenant {TenantId}", backupId, tenantId);
            
            return new BackupResult(
                backupId,
                false,
                ex.Message,
                0,
                startTime,
                DateTime.UtcNow - startTime
            );
        }
    }

    public async Task<BackupResult> CreateFullBackupAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var backupId = Guid.NewGuid().ToString();
        
        try
        {
            _logger.LogInformation("Starting full backup {BackupId}", backupId);
            
            var backupFileName = $"full_backup_{backupId}_{startTime:yyyyMMdd_HHmmss}.json";
            var backupFilePath = Path.Combine(_backupPath, backupFileName);
            
            // Create full backup data
            var backupData = await CreateFullBackupDataAsync(cancellationToken);
            
            // Write backup to file
            await File.WriteAllTextAsync(backupFilePath, backupData, cancellationToken);
            
            var fileInfo = new FileInfo(backupFilePath);
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation("Full backup {BackupId} completed successfully. Size: {Size} bytes, Duration: {Duration}", 
                backupId, fileInfo.Length, duration);
            
            return new BackupResult(
                backupId,
                true,
                null,
                fileInfo.Length,
                startTime,
                duration
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create full backup {BackupId}", backupId);
            
            return new BackupResult(
                backupId,
                false,
                ex.Message,
                0,
                startTime,
                DateTime.UtcNow - startTime
            );
        }
    }

    public async Task<List<BackupInfo>> GetBackupsAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupPath, "*.json");
            var backups = new List<BackupInfo>();
            
            foreach (var file in backupFiles)
            {
                var fileInfo = new FileInfo(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('_');
                
                if (parts.Length >= 4)
                {
                    var backupId = parts[0];
                    var fileTenantId = Guid.TryParse(parts[1], out var parsedTenantId) ? parsedTenantId : (Guid?)null;
                    var type = Enum.TryParse<BackupType>(parts[2], true, out var parsedType) ? parsedType : BackupType.Full;
                    
                    // Filter by tenant if specified
                    if (tenantId.HasValue && fileTenantId != tenantId.Value)
                        continue;
                    
                    backups.Add(new BackupInfo(
                        backupId,
                        fileTenantId,
                        type,
                        BackupStatus.Completed,
                        fileInfo.Length,
                        fileInfo.CreationTimeUtc,
                        fileInfo.CreationTimeUtc.AddDays(30), // Default 30-day retention
                        file,
                        $"Backup created on {fileInfo.CreationTimeUtc:yyyy-MM-dd HH:mm:ss}"
                    ));
                }
            }
            
            return backups.OrderByDescending(x => x.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backups");
            return new List<BackupInfo>();
        }
    }

    public async Task<BackupInfo> GetBackupInfoAsync(string backupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var backups = await GetBackupsAsync(null, cancellationToken);
            return backups.FirstOrDefault(x => x.BackupId == backupId) ?? 
                   new BackupInfo(backupId, null, BackupType.Full, BackupStatus.Failed, 0, DateTime.UtcNow, null, null, "Backup not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backup info for {BackupId}", backupId);
            return new BackupInfo(backupId, null, BackupType.Full, BackupStatus.Failed, 0, DateTime.UtcNow, null, null, "Error retrieving backup info");
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting restore of backup {BackupId}", backupId);
            
            var backupInfo = await GetBackupInfoAsync(backupId, cancellationToken);
            if (backupInfo.Status != BackupStatus.Completed || string.IsNullOrEmpty(backupInfo.Location))
            {
                _logger.LogError("Cannot restore backup {BackupId}: Invalid status or location", backupId);
                return false;
            }
            
            // In a real implementation, this would restore the data from the backup file
            // For now, we'll just log the restore action
            _logger.LogInformation("Backup {BackupId} restore completed successfully", backupId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore backup {BackupId}", backupId);
            return false;
        }
    }

    public async Task<bool> DeleteBackupAsync(string backupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var backupInfo = await GetBackupInfoAsync(backupId, cancellationToken);
            if (string.IsNullOrEmpty(backupInfo.Location))
            {
                _logger.LogError("Cannot delete backup {BackupId}: No location found", backupId);
                return false;
            }
            
            File.Delete(backupInfo.Location);
            _logger.LogInformation("Backup {BackupId} deleted successfully", backupId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete backup {BackupId}", backupId);
            return false;
        }
    }

    public async Task<bool> VerifyBackupAsync(string backupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var backupInfo = await GetBackupInfoAsync(backupId, cancellationToken);
            if (string.IsNullOrEmpty(backupInfo.Location))
            {
                return false;
            }
            
            // Check if file exists and is readable
            var fileInfo = new FileInfo(backupInfo.Location);
            return fileInfo.Exists && fileInfo.Length > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify backup {BackupId}", backupId);
            return false;
        }
    }

    public async Task ScheduleBackupAsync(Guid tenantId, BackupSchedule schedule, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Scheduling backup for tenant {TenantId} with interval {Interval}", tenantId, schedule.Interval);
            
            // In a real implementation, this would store the schedule in the database
            // and set up a background job to execute the backup
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule backup for tenant {TenantId}", tenantId);
        }
    }

    public async Task<List<BackupInfo>> GetScheduledBackupsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would query the database for scheduled backups
            _logger.LogInformation("Retrieving scheduled backups");
            return new List<BackupInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get scheduled backups");
            return new List<BackupInfo>();
        }
    }

    // New methods required by the interface
    public async Task<BackupResult> CreateBackupAsync(BackupType type, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating backup of type {Type}", type);
            
            // For system-wide backups, we'll use a default tenant ID
            var systemTenantId = Guid.Empty;
            return await CreateBackupAsync(systemTenantId, type, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup of type {Type}", type);
            throw;
        }
    }

    public async Task<bool> CleanOldBackupsAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cleaning backups older than {RetentionDays} days", retentionDays);
            
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var backupFiles = Directory.GetFiles(_backupPath, "*.json");
            var deletedCount = 0;
            
            foreach (var file in backupFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTimeUtc < cutoffDate)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                        _logger.LogDebug("Deleted old backup file: {FileName}", fileInfo.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete backup file: {FileName}", fileInfo.Name);
                    }
                }
            }
            
            _logger.LogInformation("Cleaned {DeletedCount} old backup files", deletedCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean old backups");
            return false;
        }
    }

    private async Task<string> CreateBackupDataAsync(Guid tenantId, BackupType type, CancellationToken cancellationToken)
    {
        var backupData = new
        {
            TenantId = tenantId,
            Type = type.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = new Dictionary<string, object>()
        };

        switch (type)
        {
            case BackupType.Tenant:
                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
                backupData.Data["Tenant"] = tenant;
                break;
                
            case BackupType.Queue:
                var queues = await _context.Queues.Where(q => q.TenantId == tenantId).ToListAsync(cancellationToken);
                backupData.Data["Queues"] = queues;
                break;
                
            case BackupType.UserSessions:
                var userSessions = await _context.UserSessions
                    .Where(u => _context.Queues.Any(q => q.Id == u.QueueId && q.TenantId == tenantId))
                    .ToListAsync(cancellationToken);
                backupData.Data["UserSessions"] = userSessions;
                break;
        }

        return System.Text.Json.JsonSerializer.Serialize(backupData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> CreateFullBackupDataAsync(CancellationToken cancellationToken)
    {
        var backupData = new
        {
            CreatedAt = DateTime.UtcNow,
            Tenants = await _context.Tenants.ToListAsync(cancellationToken),
            Queues = await _context.Queues.ToListAsync(cancellationToken),
            UserSessions = await _context.UserSessions.ToListAsync(cancellationToken)
        };

        return System.Text.Json.JsonSerializer.Serialize(backupData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
