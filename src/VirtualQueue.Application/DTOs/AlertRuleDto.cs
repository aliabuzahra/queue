namespace VirtualQueue.Application.DTOs;

/// <summary>
/// Data transfer object for alert rules.
/// </summary>
public record AlertRuleDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    string Condition,
    string Severity,
    bool IsEnabled,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Request model for creating an alert rule.
/// </summary>
public record CreateAlertRuleRequest(
    string Name,
    string Description,
    string Condition,
    string Severity,
    bool IsEnabled = true
);

/// <summary>
/// Request model for updating an alert rule.
/// </summary>
public record UpdateAlertRuleRequest(
    string Name,
    string Description,
    string Condition,
    string Severity,
    bool IsEnabled
);
