using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a webhook configuration in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity manages webhook configurations for external
/// system notifications and integrations.
/// </remarks>
public class Webhook : BaseEntity
{
    #region Constants
    private const int MaxNameLength = 100;
    private const int MaxUrlLength = 500;
    private const int MaxDescriptionLength = 500;
    private const int MaxHeadersLength = 2000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this webhook.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the name of the webhook.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the description of the webhook.
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Gets the URL where the webhook should be sent.
    /// </summary>
    public string Url { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the events that trigger this webhook.
    /// </summary>
    public string Events { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the headers to include with the webhook.
    /// </summary>
    public string? Headers { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this webhook is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Gets the number of retry attempts for failed deliveries.
    /// </summary>
    public int RetryCount { get; private set; } = 3;
    
    /// <summary>
    /// Gets the timeout for webhook delivery in seconds.
    /// </summary>
    public int TimeoutSeconds { get; private set; } = 30;
    
    /// <summary>
    /// Gets the secret key for webhook signature verification.
    /// </summary>
    public string? SecretKey { get; private set; }
    
    /// <summary>
    /// Gets the date and time of the last delivery.
    /// </summary>
    public DateTime? LastDeliveredAt { get; private set; }
    
    /// <summary>
    /// Gets the status of the last delivery.
    /// </summary>
    public WebhookDeliveryStatus? LastDeliveryStatus { get; private set; }
    
    /// <summary>
    /// Gets the total number of successful deliveries.
    /// </summary>
    public long SuccessCount { get; private set; } = 0;
    
    /// <summary>
    /// Gets the total number of failed deliveries.
    /// </summary>
    public long FailureCount { get; private set; } = 0;
    
    /// <summary>
    /// Gets the user identifier who created this webhook.
    /// </summary>
    public string? CreatedBy { get; private set; }
    #endregion

    #region Constructors
    private Webhook() { } // EF Core constructor

    public Webhook(
        Guid tenantId,
        string name,
        string url,
        string events,
        string? description = null,
        string? headers = null,
        int retryCount = 3,
        int timeoutSeconds = 30,
        string? secretKey = null,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
        
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        
        if (url.Length > MaxUrlLength)
            throw new ArgumentException($"URL cannot exceed {MaxUrlLength} characters", nameof(url));
        
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        
        if (!string.IsNullOrEmpty(headers) && headers.Length > MaxHeadersLength)
            throw new ArgumentException($"Headers cannot exceed {MaxHeadersLength} characters", nameof(headers));
        
        if (retryCount < 0)
            throw new ArgumentException("Retry count cannot be negative", nameof(retryCount));
        
        if (timeoutSeconds <= 0)
            throw new ArgumentException("Timeout must be positive", nameof(timeoutSeconds));

        TenantId = tenantId;
        Name = name;
        Url = url;
        Events = events;
        Description = description;
        Headers = headers;
        RetryCount = retryCount;
        TimeoutSeconds = timeoutSeconds;
        SecretKey = secretKey;
        CreatedBy = createdBy;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the webhook name.
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
    /// Updates the webhook URL.
    /// </summary>
    /// <param name="url">The new URL.</param>
    public void UpdateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        
        if (url.Length > MaxUrlLength)
            throw new ArgumentException($"URL cannot exceed {MaxUrlLength} characters", nameof(url));
        
        Url = url;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the webhook events.
    /// </summary>
    /// <param name="events">The new events.</param>
    public void UpdateEvents(string events)
    {
        if (string.IsNullOrWhiteSpace(events))
            throw new ArgumentException("Events cannot be null or empty", nameof(events));
        
        Events = events;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the webhook headers.
    /// </summary>
    /// <param name="headers">The new headers.</param>
    public void UpdateHeaders(string? headers)
    {
        if (!string.IsNullOrEmpty(headers) && headers.Length > MaxHeadersLength)
            throw new ArgumentException($"Headers cannot exceed {MaxHeadersLength} characters", nameof(headers));
        
        Headers = headers;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the webhook retry count.
    /// </summary>
    /// <param name="retryCount">The new retry count.</param>
    public void UpdateRetryCount(int retryCount)
    {
        if (retryCount < 0)
            throw new ArgumentException("Retry count cannot be negative", nameof(retryCount));
        
        RetryCount = retryCount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the webhook timeout.
    /// </summary>
    /// <param name="timeoutSeconds">The new timeout in seconds.</param>
    public void UpdateTimeout(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
            throw new ArgumentException("Timeout must be positive", nameof(timeoutSeconds));
        
        TimeoutSeconds = timeoutSeconds;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the webhook.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the webhook.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records a successful webhook delivery.
    /// </summary>
    public void RecordSuccess()
    {
        SuccessCount++;
        LastDeliveredAt = DateTime.UtcNow;
        LastDeliveryStatus = WebhookDeliveryStatus.Success;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records a failed webhook delivery.
    /// </summary>
    public void RecordFailure()
    {
        FailureCount++;
        LastDeliveredAt = DateTime.UtcNow;
        LastDeliveryStatus = WebhookDeliveryStatus.Failure;
        MarkAsUpdated();
    }
    #endregion
}

/// <summary>
/// Represents the status of a webhook delivery.
/// </summary>
public enum WebhookDeliveryStatus
{
    Success,
    Failure,
    Timeout,
    Retrying
}
