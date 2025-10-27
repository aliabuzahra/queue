using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a backup record in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity tracks backup operations and their status
/// for data protection and disaster recovery.
/// </remarks>
public class Backup : BaseEntity
{
    #region Constants
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MaxLocationLength = 500;
    private const int MaxErrorMessageLength = 1000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this backup.
    /// </summary>
    public Guid? TenantId { get; private set; }
    
    /// <summary>
    /// Gets the name of the backup.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the description of the backup.
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Gets the type of backup.
    /// </summary>
    public BackupType Type { get; private set; }
    
    /// <summary>
    /// Gets the status of the backup.
    /// </summary>
    public BackupStatus Status { get; private set; } = BackupStatus.Pending;
    
    /// <summary>
    /// Gets the size of the backup in bytes.
    /// </summary>
    public long SizeBytes { get; private set; } = 0;
    
    /// <summary>
    /// Gets the location where the backup is stored.
    /// </summary>
    public string? Location { get; private set; }
    
    /// <summary>
    /// Gets the date and time when the backup expires.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }
    
    /// <summary>
    /// Gets the date and time when the backup was started.
    /// </summary>
    public DateTime? StartedAt { get; private set; }
    
    /// <summary>
    /// Gets the date and time when the backup was completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }
    
    /// <summary>
    /// Gets the duration of the backup operation.
    /// </summary>
    public TimeSpan? Duration { get; private set; }
    
    /// <summary>
    /// Gets the error message if the backup failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }
    
    /// <summary>
    /// Gets the user identifier who initiated this backup.
    /// </summary>
    public string? CreatedBy { get; private set; }
    
    /// <summary>
    /// Gets additional metadata for this backup.
    /// </summary>
    public string? Metadata { get; private set; }
    
    /// <summary>
    /// Gets the checksum of the backup file.
    /// </summary>
    public string? Checksum { get; private set; }
    #endregion

    #region Constructors
    private Backup() { } // EF Core constructor

    public Backup(
        string name,
        BackupType type,
        Guid? tenantId = null,
        string? description = null,
        DateTime? expiresAt = null,
        string? createdBy = null,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
        
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));

        Name = name;
        Type = type;
        TenantId = tenantId;
        Description = description;
        ExpiresAt = expiresAt;
        CreatedBy = createdBy;
        Metadata = metadata;
        StartedAt = DateTime.UtcNow;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the backup name.
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
    /// Updates the backup description.
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
    /// Updates the backup status to in progress.
    /// </summary>
    public void MarkAsInProgress()
    {
        Status = BackupStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the backup as completed successfully.
    /// </summary>
    /// <param name="location">The location where the backup is stored.</param>
    /// <param name="sizeBytes">The size of the backup in bytes.</param>
    /// <param name="checksum">The checksum of the backup file.</param>
    public void MarkAsCompleted(string location, long sizeBytes, string? checksum = null)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location cannot be null or empty", nameof(location));
        
        if (location.Length > MaxLocationLength)
            throw new ArgumentException($"Location cannot exceed {MaxLocationLength} characters", nameof(location));
        
        if (sizeBytes < 0)
            throw new ArgumentException("Size cannot be negative", nameof(sizeBytes));

        Status = BackupStatus.Completed;
        Location = location;
        SizeBytes = sizeBytes;
        Checksum = checksum;
        CompletedAt = DateTime.UtcNow;
        
        if (StartedAt.HasValue)
        {
            Duration = CompletedAt.Value.Subtract(StartedAt.Value);
        }
        
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the backup as failed.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    public void MarkAsFailed(string? errorMessage = null)
    {
        Status = BackupStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        
        if (StartedAt.HasValue)
        {
            Duration = CompletedAt.Value.Subtract(StartedAt.Value);
        }
        
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the backup expiration date.
    /// </summary>
    /// <param name="expiresAt">The new expiration date.</param>
    public void UpdateExpiration(DateTime? expiresAt)
    {
        ExpiresAt = expiresAt;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if the backup is expired.
    /// </summary>
    /// <returns>True if the backup is expired, false otherwise.</returns>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the backup is completed successfully.
    /// </summary>
    /// <returns>True if the backup is completed, false otherwise.</returns>
    public bool IsCompleted()
    {
        return Status == BackupStatus.Completed;
    }
    #endregion
}

/// <summary>
/// Represents the type of backup.
/// </summary>
public enum BackupType
{
    Full,
    Tenant,
    Queue,
    UserSessions,
    Analytics,
    Configuration
}

/// <summary>
/// Represents the status of a backup.
/// </summary>
public enum BackupStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Expired,
    Corrupted
}
