using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents an alert configuration in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity manages alert configurations for monitoring
/// system health and business metrics.
/// </remarks>
public class Alert : BaseEntity
{
    #region Constants
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MaxConditionLength = 1000;
    private const int MaxMessageLength = 1000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this alert.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the name of the alert.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the description of the alert.
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Gets the type of alert.
    /// </summary>
    public AlertType Type { get; private set; }
    
    /// <summary>
    /// Gets the severity level of the alert.
    /// </summary>
    public AlertSeverity Severity { get; private set; }
    
    /// <summary>
    /// Gets the condition that triggers this alert.
    /// </summary>
    public string Condition { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the message to send when the alert is triggered.
    /// </summary>
    public string Message { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets a value indicating whether this alert is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Gets the notification channels for this alert.
    /// </summary>
    public string? NotificationChannels { get; private set; }
    
    /// <summary>
    /// Gets the cooldown period in minutes before the alert can be triggered again.
    /// </summary>
    public int CooldownMinutes { get; private set; } = 15;
    
    /// <summary>
    /// Gets the date and time when the alert was last triggered.
    /// </summary>
    public DateTime? LastTriggeredAt { get; private set; }
    
    /// <summary>
    /// Gets the number of times this alert has been triggered.
    /// </summary>
    public long TriggerCount { get; private set; } = 0;
    
    /// <summary>
    /// Gets the user identifier who created this alert.
    /// </summary>
    public string? CreatedBy { get; private set; }
    
    /// <summary>
    /// Gets additional metadata for this alert.
    /// </summary>
    public string? Metadata { get; private set; }
    #endregion

    #region Constructors
    private Alert() { } // EF Core constructor

    public Alert(
        Guid tenantId,
        string name,
        AlertType type,
        AlertSeverity severity,
        string condition,
        string message,
        string? description = null,
        string? notificationChannels = null,
        int cooldownMinutes = 15,
        string? createdBy = null,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
        
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty", nameof(condition));
        
        if (condition.Length > MaxConditionLength)
            throw new ArgumentException($"Condition cannot exceed {MaxConditionLength} characters", nameof(condition));
        
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        
        if (message.Length > MaxMessageLength)
            throw new ArgumentException($"Message cannot exceed {MaxMessageLength} characters", nameof(message));
        
        if (cooldownMinutes < 0)
            throw new ArgumentException("Cooldown minutes cannot be negative", nameof(cooldownMinutes));

        TenantId = tenantId;
        Name = name;
        Type = type;
        Severity = severity;
        Condition = condition;
        Message = message;
        Description = description;
        NotificationChannels = notificationChannels;
        CooldownMinutes = cooldownMinutes;
        CreatedBy = createdBy;
        Metadata = metadata;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the alert name.
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
    /// Updates the alert condition.
    /// </summary>
    /// <param name="condition">The new condition.</param>
    public void UpdateCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be null or empty", nameof(condition));
        
        if (condition.Length > MaxConditionLength)
            throw new ArgumentException($"Condition cannot exceed {MaxConditionLength} characters", nameof(condition));
        
        Condition = condition;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the alert message.
    /// </summary>
    /// <param name="message">The new message.</param>
    public void UpdateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        
        if (message.Length > MaxMessageLength)
            throw new ArgumentException($"Message cannot exceed {MaxMessageLength} characters", nameof(message));
        
        Message = message;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the alert severity.
    /// </summary>
    /// <param name="severity">The new severity.</param>
    public void UpdateSeverity(AlertSeverity severity)
    {
        Severity = severity;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the alert cooldown period.
    /// </summary>
    /// <param name="cooldownMinutes">The new cooldown period in minutes.</param>
    public void UpdateCooldown(int cooldownMinutes)
    {
        if (cooldownMinutes < 0)
            throw new ArgumentException("Cooldown minutes cannot be negative", nameof(cooldownMinutes));
        
        CooldownMinutes = cooldownMinutes;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the alert.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the alert.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records that the alert was triggered.
    /// </summary>
    public void RecordTrigger()
    {
        TriggerCount++;
        LastTriggeredAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if the alert can be triggered (not in cooldown period).
    /// </summary>
    /// <returns>True if the alert can be triggered, false otherwise.</returns>
    public bool CanTrigger()
    {
        if (!LastTriggeredAt.HasValue)
            return true;
        
        return DateTime.UtcNow.Subtract(LastTriggeredAt.Value).TotalMinutes >= CooldownMinutes;
    }
    #endregion
}

/// <summary>
/// Represents the type of alert.
/// </summary>
public enum AlertType
{
    System,
    Business,
    Performance,
    Security,
    Custom
}

/// <summary>
/// Represents the severity level of an alert.
/// </summary>
public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}
