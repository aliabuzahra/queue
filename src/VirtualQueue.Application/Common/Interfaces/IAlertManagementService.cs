namespace VirtualQueue.Application.Common.Interfaces;

public interface IAlertManagementService
{
    // Alert Rules (what controllers expect)
    Task<AlertRuleDto> CreateAlertRuleAsync(Guid tenantId, string name, string description, string metric, string condition, double threshold, string severity, string? notificationChannels = null, CancellationToken cancellationToken = default);
    Task<AlertRuleDto?> GetAlertRuleAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default);
    Task<List<AlertRuleDto>> GetAllAlertRulesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<AlertRuleDto> UpdateAlertRuleAsync(Guid tenantId, Guid ruleId, string name, string description, string metric, string condition, double threshold, string severity, string? notificationChannels = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAlertRuleAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default);
    Task<bool> TriggerAlertAsync(Guid tenantId, Guid alertId, string message, CancellationToken cancellationToken = default);
    Task<bool> ResolveAlertAsync(Guid tenantId, Guid alertId, string resolution, CancellationToken cancellationToken = default);
    Task<List<VirtualQueue.Application.DTOs.AlertDto>> GetActiveAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<VirtualQueue.Application.DTOs.AlertDto>> GetAlertHistoryAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    // Original Alert methods
    Task<AlertDto> CreateAlertAsync(CreateAlertRequest request, CancellationToken cancellationToken = default);
    Task<AlertDto> UpdateAlertAsync(Guid alertId, UpdateAlertRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAlertAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task<AlertDto?> GetAlertAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task<List<AlertDto>> GetAlertsAsync(Guid tenantId, AlertSeverity? severity = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<bool> TriggerAlertAsync(Guid alertId, AlertTriggerData triggerData, CancellationToken cancellationToken = default);
    Task<bool> ResolveAlertAsync(Guid alertId, string? resolution = null, CancellationToken cancellationToken = default);
    Task<List<AlertHistory>> GetAlertHistoryByAlertIdAsync(Guid alertId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<bool> TestAlertAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task<List<AlertDto>> GetAlertsByConditionAsync(AlertCondition condition, CancellationToken cancellationToken = default);
}

public record AlertDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    AlertType Type,
    AlertSeverity Severity,
    AlertCondition Condition,
    List<AlertAction> Actions,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy
);

public record CreateAlertRequest(
    Guid TenantId,
    string Name,
    string Description,
    AlertType Type,
    AlertSeverity Severity,
    AlertCondition Condition,
    List<AlertAction> Actions,
    bool IsActive = true,
    string? CreatedBy = null
);

public record UpdateAlertRequest(
    string? Name,
    string? Description,
    AlertSeverity? Severity,
    AlertCondition? Condition,
    List<AlertAction>? Actions,
    bool? IsActive
);

public record AlertCondition(
    string Metric,
    AlertOperator Operator,
    double Threshold,
    TimeSpan? Duration = null,
    Dictionary<string, object>? Metadata = null
);

public record AlertAction(
    AlertActionType Type,
    string Target,
    string? Message = null,
    Dictionary<string, object>? Parameters = null
);

public record AlertTriggerData(
    string Metric,
    double Value,
    DateTime Timestamp,
    Dictionary<string, object>? Metadata = null
);

public record AlertHistory(
    Guid Id,
    Guid AlertId,
    AlertTriggerData TriggerData,
    bool WasTriggered,
    DateTime Timestamp,
    string? Message = null
);

public enum AlertType
{
    QueueLength,
    WaitTime,
    Throughput,
    ErrorRate,
    SystemHealth,
    Custom
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AlertOperator
{
    GreaterThan,
    LessThan,
    Equal,
    NotEqual,
    GreaterThanOrEqual,
    LessThanOrEqual
}

public enum AlertActionType
{
    Email,
    Sms,
    Webhook,
    Slack,
    Teams
}

// Additional types needed by controllers
public record AlertRuleDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    string Metric,
    string Condition,
    double Threshold,
    string Severity,
    string? NotificationChannels,
    bool IsEnabled,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateAlertRuleRequest(
    string Name,
    string Description,
    string Metric,
    string Condition,
    double Threshold,
    string Severity,
    string? NotificationChannels = null
);

public record UpdateAlertRuleRequest(
    string Name,
    string Description,
    string Metric,
    string Condition,
    double Threshold,
    string Severity,
    string? NotificationChannels
);

public record AuthenticationResult(
    bool Success,
    string? Token,
    string? UserId,
    List<string> Roles,
    string? ErrorMessage
);

public record RuleContext(
    Guid TenantId,
    Guid? QueueId,
    string? UserIdentifier,
    Dictionary<string, object> Variables,
    DateTime Timestamp
);
