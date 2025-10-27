namespace VirtualQueue.Application.DTOs;

public record QueueTemplateDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Description,
    string TemplateType,
    int MaxConcurrentUsers,
    int ReleaseRatePerMinute,
    string? ScheduleJson,
    string? BusinessRulesJson,
    string? NotificationSettingsJson,
    bool IsPublic,
    bool IsActive,
    int UsageCount,
    Dictionary<string, string> Metadata,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

public record CreateQueueTemplateRequest(
    string Name,
    string Description,
    string TemplateType,
    int MaxConcurrentUsers,
    int ReleaseRatePerMinute,
    string? ScheduleJson = null,
    string? BusinessRulesJson = null,
    string? NotificationSettingsJson = null,
    bool IsPublic = false);

public record UpdateQueueTemplateRequest(
    string? Name,
    string? Description,
    int? MaxConcurrentUsers,
    int? ReleaseRatePerMinute,
    string? ScheduleJson,
    string? BusinessRulesJson,
    string? NotificationSettingsJson);

public record QueueTemplateUsageRequest(
    Guid TemplateId,
    string QueueName,
    string? Description = null);

public record QueueTemplateSearchRequest(
    string? SearchTerm,
    string? TemplateType,
    bool? IsPublic,
    bool? IsActive);

