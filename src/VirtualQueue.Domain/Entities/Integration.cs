using VirtualQueue.Domain.Common;

namespace VirtualQueue.Domain.Entities;

/// <summary>
/// Represents a third-party integration in the virtual queue system.
/// </summary>
/// <remarks>
/// This entity manages integrations with external systems
/// and services for enhanced functionality.
/// </remarks>
public class Integration : BaseEntity
{
    #region Constants
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MaxConfigurationLength = 5000;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the tenant identifier for this integration.
    /// </summary>
    public Guid TenantId { get; private set; }
    
    /// <summary>
    /// Gets the name of the integration.
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets the description of the integration.
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Gets the type of integration.
    /// </summary>
    public IntegrationType Type { get; private set; }
    
    /// <summary>
    /// Gets the provider of the integration.
    /// </summary>
    public IntegrationProvider Provider { get; private set; }
    
    /// <summary>
    /// Gets the configuration for this integration.
    /// </summary>
    public string Configuration { get; private set; } = string.Empty;
    
    /// <summary>
    /// Gets a value indicating whether this integration is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Gets the status of this integration.
    /// </summary>
    public IntegrationStatus Status { get; private set; } = IntegrationStatus.Pending;
    
    /// <summary>
    /// Gets the date and time of the last test.
    /// </summary>
    public DateTime? LastTestedAt { get; private set; }
    
    /// <summary>
    /// Gets the result of the last test.
    /// </summary>
    public IntegrationTestResult? LastTestResult { get; private set; }
    
    /// <summary>
    /// Gets the user identifier who created this integration.
    /// </summary>
    public string? CreatedBy { get; private set; }
    
    /// <summary>
    /// Gets additional metadata for this integration.
    /// </summary>
    public string? Metadata { get; private set; }
    #endregion

    #region Constructors
    private Integration() { } // EF Core constructor

    public Integration(
        Guid tenantId,
        string name,
        IntegrationType type,
        IntegrationProvider provider,
        string configuration,
        string? description = null,
        string? createdBy = null,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters", nameof(name));
        
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        
        if (string.IsNullOrWhiteSpace(configuration))
            throw new ArgumentException("Configuration cannot be null or empty", nameof(configuration));
        
        if (configuration.Length > MaxConfigurationLength)
            throw new ArgumentException($"Configuration cannot exceed {MaxConfigurationLength} characters", nameof(configuration));

        TenantId = tenantId;
        Name = name;
        Type = type;
        Provider = provider;
        Configuration = configuration;
        Description = description;
        CreatedBy = createdBy;
        Metadata = metadata;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the integration name.
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
    /// Updates the integration description.
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
    /// Updates the integration configuration.
    /// </summary>
    /// <param name="configuration">The new configuration.</param>
    public void UpdateConfiguration(string configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration))
            throw new ArgumentException("Configuration cannot be null or empty", nameof(configuration));
        
        if (configuration.Length > MaxConfigurationLength)
            throw new ArgumentException($"Configuration cannot exceed {MaxConfigurationLength} characters", nameof(configuration));
        
        Configuration = configuration;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the integration.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        Status = IntegrationStatus.Active;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the integration.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        Status = IntegrationStatus.Inactive;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the integration status.
    /// </summary>
    /// <param name="status">The new status.</param>
    public void UpdateStatus(IntegrationStatus status)
    {
        Status = status;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records a test result for the integration.
    /// </summary>
    /// <param name="result">The test result.</param>
    public void RecordTestResult(IntegrationTestResult result)
    {
        LastTestedAt = DateTime.UtcNow;
        LastTestResult = result;
        
        if (result == IntegrationTestResult.Success)
        {
            Status = IntegrationStatus.Active;
        }
        else
        {
            Status = IntegrationStatus.Error;
        }
        
        MarkAsUpdated();
    }
    #endregion
}

/// <summary>
/// Represents the type of integration.
/// </summary>
public enum IntegrationType
{
    Payment,
    Notification,
    Analytics,
    CRM,
    ERP,
    Custom
}

/// <summary>
/// Represents the provider of an integration.
/// </summary>
public enum IntegrationProvider
{
    Stripe,
    PayPal,
    SendGrid,
    Twilio,
    Salesforce,
    HubSpot,
    Custom
}

/// <summary>
/// Represents the status of an integration.
/// </summary>
public enum IntegrationStatus
{
    Pending,
    Active,
    Inactive,
    Error,
    Testing
}

/// <summary>
/// Represents the result of an integration test.
/// </summary>
public enum IntegrationTestResult
{
    Success,
    Failure,
    Timeout,
    ConfigurationError
}
