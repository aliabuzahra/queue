namespace VirtualQueue.Application.DTOs;

/// <summary>
/// Data transfer object for webhook subscriptions.
/// </summary>
public record WebhookSubscriptionDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Url,
    string EventType,
    bool IsActive,
    string? Secret,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Request model for creating a webhook subscription.
/// </summary>
public record CreateWebhookSubscriptionRequest(
    string Name,
    string Url,
    string EventType,
    bool IsActive = true,
    string? Secret = null
);

/// <summary>
/// Request model for updating a webhook subscription.
/// </summary>
public record UpdateWebhookSubscriptionRequest(
    string Name,
    string Url,
    string EventType,
    bool IsActive,
    string? Secret
);
