using VirtualQueue.Domain.Common;
using VirtualQueue.Domain.Enums;
using VirtualQueue.Domain.Events;
using VirtualQueue.Domain.ValueObjects;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a virtual queue in the system.
/// </summary>
/// <remarks>
/// This class manages user sessions in a virtual queue with configurable
/// capacity, release rates, and scheduling options.
/// </remarks>
public class Queue : BaseEntity
{
    #region Constants
    private const int MinNameLength = 1;
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MinConcurrentUsers = 1;
    private const int MaxConcurrentUsersLimit = 10000;
    private const int MinReleaseRate = 1;
    private const int MaxReleaseRate = 1000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this queue.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the name of the queue.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the description of the queue.
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the maximum number of concurrent users that can be served.
    /// </summary>
    public int MaxConcurrentUsers { get; private set; }
    
    /// <summary>
    /// Gets the rate at which users are released per minute.
    /// </summary>
    public int ReleaseRatePerMinute { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether the queue is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Gets the date and time of the last user release.
    /// </summary>
    public DateTime? LastReleaseAt { get; private set; }
    
    /// <summary>
    /// Gets the schedule configuration for this queue.
    /// </summary>
    public QueueSchedule? Schedule { get; private set; }
    
    /// <summary>
    /// Gets the collection of users in this queue.
    /// </summary>
    private readonly List<UserSession> _users = new();
    public IReadOnlyCollection<UserSession> Users => _users.AsReadOnly();

    /// <summary>
    /// Gets the collection of domain events for this queue.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Queue"/> class.
    /// This constructor is used by Entity Framework Core.
    /// </summary>
    private Queue() { }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes a new instance of the <see cref="Queue"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="name">The name of the queue.</param>
    /// <param name="description">The description of the queue.</param>
    /// <param name="maxConcurrentUsers">The maximum number of concurrent users.</param>
    /// <param name="releaseRatePerMinute">The rate at which users are released per minute.</param>
    /// <param name="schedule">The schedule configuration for the queue.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any of the required parameters are invalid.
    /// </exception>
    public Queue(Guid tenantId, string name, string description, int maxConcurrentUsers, int releaseRatePerMinute, QueueSchedule? schedule = null)
    {
        ValidateInputs(tenantId, name, description, maxConcurrentUsers, releaseRatePerMinute);

        TenantId = tenantId;
        Name = name;
        Description = description;
        MaxConcurrentUsers = maxConcurrentUsers;
        ReleaseRatePerMinute = releaseRatePerMinute;
        Schedule = schedule;
        
        _domainEvents.Add(new QueueCreatedEvent(Id, TenantId, Name));
    }

    /// <summary>
    /// Updates the name of the queue.
    /// </summary>
    /// <param name="name">The new name for the queue.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the name is null, empty, or exceeds maximum length.
    /// </exception>
    public void UpdateName(string name)
    {
        ValidateName(name);

        Name = name;
        MarkAsUpdated();
    }

    public void UpdateDescription(string description)
    {
        Description = description;
        MarkAsUpdated();
    }

    public void UpdateConfiguration(int maxConcurrentUsers, int releaseRatePerMinute)
    {
        if (maxConcurrentUsers <= 0)
            throw new ArgumentException("MaxConcurrentUsers must be greater than 0", nameof(maxConcurrentUsers));
        
        if (releaseRatePerMinute <= 0)
            throw new ArgumentException("ReleaseRatePerMinute must be greater than 0", nameof(releaseRatePerMinute));

        MaxConcurrentUsers = maxConcurrentUsers;
        ReleaseRatePerMinute = releaseRatePerMinute;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
        
        _domainEvents.Add(new QueueDeactivatedEvent(Id, TenantId));
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
        
        _domainEvents.Add(new QueueActivatedEvent(Id, TenantId));
    }

    public UserSession EnqueueUser(string userIdentifier, string? metadata = null, QueuePriority priority = QueuePriority.Normal)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentException("UserIdentifier cannot be null or empty", nameof(userIdentifier));

        if (!IsActive)
            throw new InvalidOperationException("Cannot enqueue user to inactive queue");

        var userSession = new UserSession(Id, userIdentifier, metadata, priority);
        _users.Add(userSession);
        MarkAsUpdated();
        
        _domainEvents.Add(new UserEnqueuedEvent(userSession.Id, Id, TenantId, userIdentifier));
        
        return userSession;
    }

    public void ReleaseUsers(int count)
    {
        if (count <= 0)
            return;

        // Get waiting users ordered by priority (highest first) and then by enqueue time
        var waitingUsers = _users
            .Where(u => u.Status == QueueStatus.Waiting)
            .OrderByDescending(u => u.Priority)
            .ThenBy(u => u.EnqueuedAt)
            .Take(count)
            .ToList();

        foreach (var user in waitingUsers)
        {
            user.MarkAsReleased();
        }

        LastReleaseAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public int GetWaitingUsersCount()
    {
        return _users.Count(u => u.Status == Domain.Enums.QueueStatus.Waiting);
    }

    public void SetSchedule(QueueSchedule schedule)
    {
        Schedule = schedule;
        MarkAsUpdated();
        
        _domainEvents.Add(new QueueScheduleUpdatedEvent(Id, TenantId));
    }

    public bool IsQueueAvailable(DateTime dateTime)
    {
        if (!IsActive)
            return false;

        if (Schedule == null)
            return true;

        return Schedule.IsQueueActive(dateTime);
    }

    public DateTime? GetNextActivationTime(DateTime fromDateTime)
    {
        if (Schedule == null)
            return null;

        return Schedule.GetNextActivationTime(fromDateTime);
    }

    public DateTime? GetPreviousActivationTime(DateTime fromDateTime)
    {
        if (Schedule == null)
            return null;

        return Schedule.GetPreviousActivationTime(fromDateTime);
    }

    public int GetServingUsersCount()
    {
        return _users.Count(u => u.Status == Domain.Enums.QueueStatus.Serving);
    }

    /// <summary>
    /// Clears all domain events for this queue.
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
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="name">The queue name.</param>
    /// <param name="description">The queue description.</param>
    /// <param name="maxConcurrentUsers">The maximum concurrent users.</param>
    /// <param name="releaseRatePerMinute">The release rate per minute.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when any parameter is invalid.
    /// </exception>
    private static void ValidateInputs(
        Guid tenantId,
        string name,
        string description,
        int maxConcurrentUsers,
        int releaseRatePerMinute)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            
        ValidateName(name);
        ValidateDescription(description);
        ValidateMaxConcurrentUsers(maxConcurrentUsers);
        ValidateReleaseRate(releaseRatePerMinute);
    }

    /// <summary>
    /// Validates a queue name.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the name is invalid.
    /// </exception>
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
            
        if (name.Length < MinNameLength)
            throw new ArgumentException($"Name must be at least {MinNameLength} character long", nameof(name));
            
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
    }

    /// <summary>
    /// Validates a queue description.
    /// </summary>
    /// <param name="description">The description to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the description is invalid.
    /// </exception>
    private static void ValidateDescription(string description)
    {
        if (description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
    }

    /// <summary>
    /// Validates the maximum concurrent users.
    /// </summary>
    /// <param name="maxConcurrentUsers">The value to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is invalid.
    /// </exception>
    private static void ValidateMaxConcurrentUsers(int maxConcurrentUsers)
    {
        if (maxConcurrentUsers < MinConcurrentUsers)
            throw new ArgumentException($"MaxConcurrentUsers must be at least {MinConcurrentUsers}", nameof(maxConcurrentUsers));
            
        if (maxConcurrentUsers > MaxConcurrentUsersLimit)
            throw new ArgumentException($"MaxConcurrentUsers cannot exceed {MaxConcurrentUsersLimit}", nameof(maxConcurrentUsers));
    }

    /// <summary>
    /// Validates the release rate per minute.
    /// </summary>
    /// <param name="releaseRatePerMinute">The value to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is invalid.
    /// </exception>
    private static void ValidateReleaseRate(int releaseRatePerMinute)
    {
        if (releaseRatePerMinute < MinReleaseRate)
            throw new ArgumentException($"ReleaseRatePerMinute must be at least {MinReleaseRate}", nameof(releaseRatePerMinute));
            
        if (releaseRatePerMinute > MaxReleaseRate)
            throw new ArgumentException($"ReleaseRatePerMinute cannot exceed {MaxReleaseRate}", nameof(releaseRatePerMinute));
    }
    #endregion
}
