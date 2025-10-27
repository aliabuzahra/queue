namespace VirtualQueue.Application.Common.Interfaces;

public interface IThirdPartyIntegrationService
{
    // Integration methods (what controllers expect)
    Task<IntegrationStatusDto> CreateIntegrationAsync(Guid tenantId, string name, string type, string configuration, CancellationToken cancellationToken = default);
    Task<IntegrationStatusDto?> GetIntegrationAsync(Guid tenantId, Guid integrationId, CancellationToken cancellationToken = default);
    Task<List<IntegrationStatusDto>> GetAllIntegrationsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IntegrationStatusDto> UpdateIntegrationAsync(Guid tenantId, Guid integrationId, string name, string type, string configuration, CancellationToken cancellationToken = default);
    Task<bool> DeleteIntegrationAsync(Guid tenantId, Guid integrationId, CancellationToken cancellationToken = default);
    Task<bool> TestIntegrationAsync(Guid tenantId, Guid integrationId, CancellationToken cancellationToken = default);
    Task<bool> SyncIntegrationAsync(Guid tenantId, Guid integrationId, CancellationToken cancellationToken = default);
    Task<bool> DispatchEventToIntegrationAsync(Guid tenantId, Guid integrationId, string eventType, object data, CancellationToken cancellationToken = default);
    
    // Original Integration methods
    Task<IntegrationDto> CreateIntegrationAsync(CreateIntegrationRequest request, CancellationToken cancellationToken = default);
    Task<IntegrationDto> UpdateIntegrationAsync(Guid integrationId, UpdateIntegrationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<IntegrationDto?> GetIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<List<IntegrationDto>> GetIntegrationsAsync(Guid tenantId, IntegrationType? type = null, CancellationToken cancellationToken = default);
    Task<IntegrationTestResult> TestIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<bool> EnableIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<bool> DisableIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<IntegrationStatus> GetIntegrationStatusAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<List<IntegrationEvent>> GetIntegrationEventsAsync(Guid integrationId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

public record IntegrationDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    IntegrationType Type,
    IntegrationProvider Provider,
    Dictionary<string, object> Configuration,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy
);

public record CreateIntegrationRequest(
    Guid TenantId,
    string Name,
    string Description,
    IntegrationType Type,
    IntegrationProvider Provider,
    Dictionary<string, object> Configuration,
    bool IsActive = true,
    string? CreatedBy = null
);

public record UpdateIntegrationRequest(
    string? Name,
    string? Description,
    Dictionary<string, object>? Configuration,
    bool? IsActive
);

public record IntegrationTestResult(
    Guid IntegrationId,
    bool Success,
    string? ErrorMessage,
    TimeSpan ResponseTime,
    Dictionary<string, object>? TestData,
    DateTime TestedAt
);

public record IntegrationStatus(
    Guid IntegrationId,
    bool IsConnected,
    DateTime LastConnected,
    DateTime? LastError,
    string? LastErrorMessage,
    int SuccessCount,
    int ErrorCount,
    double SuccessRate
);

public record IntegrationEvent(
    Guid Id,
    Guid IntegrationId,
    string EventType,
    string Description,
    Dictionary<string, object>? Data,
    bool Success,
    string? ErrorMessage,
    DateTime Timestamp
);

public enum IntegrationType
{
    Payment,
    Notification,
    Analytics,
    CRM,
    ERP,
    Custom
}

public enum IntegrationProvider
{
    Stripe,
    PayPal,
    Twilio,
    SendGrid,
    Salesforce,
    HubSpot,
    GoogleAnalytics,
    Custom
}

// Additional types needed by controllers
public record IntegrationStatusDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Type,
    string Status,
    DateTime LastSyncAt,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
