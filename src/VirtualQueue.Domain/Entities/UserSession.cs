using VirtualQueue.Domain.Common;
using VirtualQueue.Domain.Enums;
using VirtualQueue.Domain.Events;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a user session in a virtual queue.
/// </summary>
/// <remarks>
/// This class manages the state of a user's session within a queue,
/// including their position, status, and priority.
/// </remarks>
public class UserSession : BaseEntity
{
    #region Constants
    private const int MinUserIdentifierLength = 1;
    private const int MaxUserIdentifierLength = 255;
    private const int MaxMetadataLength = 1000;
    private const int MinPosition = 0;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the queue identifier for this user session.
    /// </summary>
    public Guid QueueId { get; private set; }
    
    /// <summary>
    /// Gets the user identifier for this session.
    /// </summary>
    public string UserIdentifier { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the current status of the user session.
    /// </summary>
    public QueueStatus Status { get; private set; } = QueueStatus.Waiting;
    
    /// <summary>
    /// Gets the priority level of the user session.
    /// </summary>
    public QueuePriority Priority { get; private set; } = QueuePriority.Normal;
    
    /// <summary>
    /// Gets the date and time when the user was enqueued.
    /// </summary>
    public DateTime EnqueuedAt { get; private set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets the date and time when the user was released from the queue.
    /// </summary>
    public DateTime? ReleasedAt { get; private set; }
    
    /// <summary>
    /// Gets the date and time when the user was served.
    /// </summary>
    public DateTime? ServedAt { get; private set; }
    
    /// <summary>
    /// Gets the current position of the user in the queue.
    /// </summary>
    public int Position { get; private set; }
    
    /// <summary>
    /// Gets the metadata associated with this user session.
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Gets the collection of domain events for this user session.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSession"/> class.
    /// This constructor is used by Entity Framework Core.
    /// </summary>
    private UserSession() { }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSession"/> class.
    /// </summary>
    /// <param name="queueId">The queue identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="metadata">Optional metadata for the session.</param>
    /// <param name="priority">The priority level for the session.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any of the required parameters are invalid.
    /// </exception>
    public UserSession(Guid queueId, string userIdentifier, string? metadata = null, QueuePriority priority = QueuePriority.Normal)
    {
        ValidateInputs(queueId, userIdentifier, metadata);

        QueueId = queueId;
        UserIdentifier = userIdentifier;
        Metadata = metadata;
        Priority = priority;
        EnqueuedAt = DateTime.UtcNow;
        Status = QueueStatus.Waiting;
    }

    /// <summary>
    /// Marks the user session as currently being served.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the user session is not in a waiting state.
    /// </exception>
    public void MarkAsServing()
    {
        if (Status != QueueStatus.Waiting)
            throw new InvalidOperationException("Only waiting users can be marked as serving");

        Status = QueueStatus.Serving;
        ServedAt = DateTime.UtcNow;
        MarkAsUpdated();
        
        _domainEvents.Add(new UserServedEvent(Id, QueueId, UserIdentifier));
    }

    public void MarkAsReleased()
    {
        if (Status == QueueStatus.Released)
            return;

        Status = QueueStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        MarkAsUpdated();
        
        _domainEvents.Add(new UserReleasedEvent(Id, QueueId, UserIdentifier));
    }

    public void MarkAsDropped()
    {
        if (Status == QueueStatus.Dropped)
            return;

        Status = QueueStatus.Dropped;
        MarkAsUpdated();
        
        _domainEvents.Add(new UserDroppedEvent(Id, QueueId, UserIdentifier));
    }

    public void UpdatePosition(int position)
    {
        if (position < 0)
            throw new ArgumentException("Position cannot be negative", nameof(position));

        Position = position;
        MarkAsUpdated();
    }

    public void UpdateMetadata(string? metadata)
    {
        Metadata = metadata;
        MarkAsUpdated();
    }

    public void UpdatePriority(QueuePriority priority)
    {
        Priority = priority;
        MarkAsUpdated();
    }

    /// <summary>
    /// Clears all domain events for this user session.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Validates all input parameters for the constructor.
    /// </summary>
    /// <param name="queueId">The queue identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <param name="metadata">The metadata.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any parameter is invalid.
    /// </exception>
    private static void ValidateInputs(Guid queueId, string userIdentifier, string? metadata)
    {
        if (queueId == Guid.Empty)
            throw new ArgumentException("Queue ID cannot be empty", nameof(queueId));
            
        ValidateUserIdentifier(userIdentifier);
        ValidateMetadata(metadata);
    }

    /// <summary>
    /// Validates a user identifier.
    /// </summary>
    /// <param name="userIdentifier">The user identifier to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the user identifier is invalid.
    /// </exception>
    private static void ValidateUserIdentifier(string userIdentifier)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentException("User identifier cannot be null or empty", nameof(userIdentifier));
            
        if (userIdentifier.Length < MinUserIdentifierLength)
            throw new ArgumentException($"User identifier must be at least {MinUserIdentifierLength} character long", nameof(userIdentifier));
            
        if (userIdentifier.Length > MaxUserIdentifierLength)
            throw new ArgumentException($"User identifier cannot exceed {MaxUserIdentifierLength} characters", nameof(userIdentifier));
    }

    /// <summary>
    /// Validates metadata.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the metadata is invalid.
    /// </exception>
    private static void ValidateMetadata(string? metadata)
    {
        if (metadata != null && metadata.Length > MaxMetadataLength)
            throw new ArgumentException($"Metadata cannot exceed {MaxMetadataLength} characters", nameof(metadata));
    }
    #endregion
}
