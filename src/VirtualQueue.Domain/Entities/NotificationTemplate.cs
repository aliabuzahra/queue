using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a notification template in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity manages notification templates for different types
/// of communications (email, SMS, WhatsApp) across tenants.
/// </remarks>
public class NotificationTemplate : BaseEntity
{
    #region Constants
    private const int MaxNameLength = 100;
    private const int MaxSubjectLength = 200;
    private const int MaxBodyLength = 5000;
    private const int MaxVariablesLength = 1000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this notification template.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the name of the notification template.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the type of notification (Email, SMS, WhatsApp).
    /// </summary>
    public NotificationType Type { get; private set; }
    
    /// <summary>
    /// Gets the subject/title of the notification.
    /// </summary>
    public string Subject { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the body content of the notification.
    /// </summary>
    public string Body { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the language code for this template.
    /// </summary>
    public string Language { get; private set; } = "en";
    
    /// <summary>
    /// Gets a value indicating whether this template is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Gets the variables used in this template.
    /// </summary>
    public string? Variables { get; private set; }
    
    /// <summary>
    /// Gets the priority of this template.
    /// </summary>
    public int Priority { get; private set; } = 100;
    
    /// <summary>
    /// Gets the category of this template.
    /// </summary>
    public string? Category { get; private set; }
    
    /// <summary>
    /// Gets the tags associated with this template.
    /// </summary>
    public string? Tags { get; private set; }
    #endregion

    #region Constructors
    private NotificationTemplate() { } // EF Core constructor

    public NotificationTemplate(
        Guid tenantId,
        string name,
        NotificationType type,
        string subject,
        string body,
        string language = "en",
        string? variables = null,
        int priority = 100,
        string? category = null,
        string? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
        
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or empty", nameof(subject));
        
        if (subject.Length > MaxSubjectLength)
            throw new ArgumentException($"Subject cannot exceed {MaxSubjectLength} characters", nameof(subject));
        
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be null or empty", nameof(body));
        
        if (body.Length > MaxBodyLength)
            throw new ArgumentException($"Body cannot exceed {MaxBodyLength} characters", nameof(body));
        
        if (!string.IsNullOrEmpty(variables) && variables.Length > MaxVariablesLength)
            throw new ArgumentException($"Variables cannot exceed {MaxVariablesLength} characters", nameof(variables));

        TenantId = tenantId;
        Name = name;
        Type = type;
        Subject = subject;
        Body = body;
        Language = language;
        Variables = variables;
        Priority = priority;
        Category = category;
        Tags = tags;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the template name.
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
    /// Updates the template content.
    /// </summary>
    /// <param name="subject">The new subject.</param>
    /// <param name="body">The new body.</param>
    public void UpdateContent(string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or empty", nameof(subject));
        
        if (subject.Length > MaxSubjectLength)
            throw new ArgumentException($"Subject cannot exceed {MaxSubjectLength} characters", nameof(subject));
        
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be null or empty", nameof(body));
        
        if (body.Length > MaxBodyLength)
            throw new ArgumentException($"Body cannot exceed {MaxBodyLength} characters", nameof(body));
        
        Subject = subject;
        Body = body;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the template variables.
    /// </summary>
    /// <param name="variables">The new variables.</param>
    public void UpdateVariables(string? variables)
    {
        if (!string.IsNullOrEmpty(variables) && variables.Length > MaxVariablesLength)
            throw new ArgumentException($"Variables cannot exceed {MaxVariablesLength} characters", nameof(variables));
        
        Variables = variables;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the template.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the template.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the template priority.
    /// </summary>
    /// <param name="priority">The new priority.</param>
    public void UpdatePriority(int priority)
    {
        if (priority < 0)
            throw new ArgumentException("Priority cannot be negative", nameof(priority));
        
        Priority = priority;
        MarkAsUpdated();
    }
    #endregion
}

/// <summary>
/// Represents the type of notification.
/// </summary>
public enum NotificationType
{
    Email,
    Sms,
    WhatsApp,
    Push
}
