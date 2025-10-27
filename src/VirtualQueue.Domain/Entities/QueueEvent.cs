using VirtualQueue.Domain.Common;
using VirtualQueue.Domain.Events;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a queue event in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity tracks all events that occur within a queue,
/// providing audit trail and analytics capabilities.
/// </remarks>
public class QueueEvent : BaseEntity
{
    #region Constants
    private const int MaxEventTypeLength = 50;
    private const int MaxDescriptionLength = 500;
    private const int MaxMetadataLength = 2000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this queue event.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the queue identifier for this event.
    /// </summary>
    public Guid QueueId { get; private set; }
    
    /// <summary>
    /// Gets the user session identifier associated with this event.
    /// </summary>
    public Guid? UserSessionId { get; private set; }
    
    /// <summary>
    /// Gets the type of event that occurred.
    /// </summary>
    public string EventType { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the description of the event.
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime EventTimestamp { get; private set; }
    
    /// <summary>
    /// Gets the user identifier who triggered the event.
    /// </summary>
    public string? UserIdentifier { get; private set; }
    
    /// <summary>
    /// Gets additional metadata associated with the event.
    /// </summary>
    public string? Metadata { get; private set; }
    
    /// <summary>
    /// Gets the IP address from which the event was triggered.
    /// </summary>
    public string? IpAddress { get; private set; }
    
    /// <summary>
    /// Gets the user agent string from the client.
    /// </summary>
    public string? UserAgent { get; private set; }
    #endregion

    #region Constructors
    private QueueEvent() { } // EF Core constructor

    public QueueEvent(
        Guid tenantId,
        Guid queueId,
        string eventType,
        string description,
        Guid? userSessionId = null,
        string? userIdentifier = null,
        string? metadata = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));
        
        if (eventType.Length > MaxEventTypeLength)
            throw new ArgumentException($"Event type cannot exceed {MaxEventTypeLength} characters", nameof(eventType));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
        
        if (description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        
        if (!string.IsNullOrEmpty(metadata) && metadata.Length > MaxMetadataLength)
            throw new ArgumentException($"Metadata cannot exceed {MaxMetadataLength} characters", nameof(metadata));

        TenantId = tenantId;
        QueueId = queueId;
        EventType = eventType;
        Description = description;
        UserSessionId = userSessionId;
        UserIdentifier = userIdentifier;
        Metadata = metadata;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        EventTimestamp = DateTime.UtcNow;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the event metadata.
    /// </summary>
    /// <param name="metadata">The new metadata.</param>
    public void UpdateMetadata(string? metadata)
    {
        if (!string.IsNullOrEmpty(metadata) && metadata.Length > MaxMetadataLength)
            throw new ArgumentException($"Metadata cannot exceed {MaxMetadataLength} characters", nameof(metadata));
        
        Metadata = metadata;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the event description.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
        
        if (description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        
        Description = description;
        MarkAsUpdated();
    }
    #endregion
}
