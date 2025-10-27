namespace VirtualQueue.Application.Common.Interfaces;

public interface IBackupService
{
    // Methods expected by controllers
    Task<BackupResult> CreateBackupAsync(BackupType type, CancellationToken cancellationToken = default);
    Task<bool> CleanOldBackupsAsync(int retentionDays, CancellationToken cancellationToken = default);
    
    // Original methods
    Task<BackupResult> CreateBackupAsync(Guid tenantId, BackupType type, CancellationToken cancellationToken = default);
    Task<BackupResult> CreateFullBackupAsync(CancellationToken cancellationToken = default);
    Task<List<BackupInfo>> GetBackupsAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<BackupInfo> GetBackupInfoAsync(string backupId, CancellationToken cancellationToken = default);
    Task<bool> RestoreBackupAsync(string backupId, CancellationToken cancellationToken = default);
    Task<bool> DeleteBackupAsync(string backupId, CancellationToken cancellationToken = default);
    Task<bool> VerifyBackupAsync(string backupId, CancellationToken cancellationToken = default);
    Task ScheduleBackupAsync(Guid tenantId, BackupSchedule schedule, CancellationToken cancellationToken = default);
    Task<List<BackupInfo>> GetScheduledBackupsAsync(CancellationToken cancellationToken = default);
}

public record BackupResult(
    string BackupId,
    bool Success,
    string? ErrorMessage,
    long SizeBytes,
    DateTime CreatedAt,
    TimeSpan Duration
);

public record BackupInfo(
    string BackupId,
    Guid? TenantId,
    BackupType Type,
    BackupStatus Status,
    long SizeBytes,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    string? Location,
    string? Description
);

public enum BackupType
{
    Full,
    Tenant,
    Queue,
    UserSessions,
    Analytics
}

public enum BackupStatus
{
    InProgress,
    Completed,
    Failed,
    Expired,
    Corrupted
}

public record BackupSchedule(
    Guid TenantId,
    BackupType Type,
    TimeSpan Interval,
    int MaxBackups,
    DateTime? NextRun,
    bool IsActive
);
