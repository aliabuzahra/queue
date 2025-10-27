using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.DTOs;

/// <summary>
/// Data transfer object for alerts.
/// </summary>
public record AlertDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    AlertType Type,
    AlertSeverity Severity,
    string Condition,
    string Message,
    bool IsActive,
    string? NotificationChannels,
    int CooldownMinutes,
    DateTime? LastTriggeredAt,
    long TriggerCount,
    string? CreatedBy,
    string? Metadata,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Request model for creating an alert.
/// </summary>
public record CreateAlertRequest(
    string Name,
    string? Description,
    AlertType Type,
    AlertSeverity Severity,
    string Condition,
    string Message,
    bool IsActive = true,
    string? NotificationChannels = null,
    int CooldownMinutes = 15,
    string? CreatedBy = null,
    string? Metadata = null
);

/// <summary>
/// Request model for updating an alert.
/// </summary>
public record UpdateAlertRequest(
    string Name,
    string? Description,
    AlertType Type,
    AlertSeverity Severity,
    string Condition,
    string Message,
    bool IsActive,
    string? NotificationChannels,
    int CooldownMinutes,
    string? CreatedBy,
    string? Metadata
);
