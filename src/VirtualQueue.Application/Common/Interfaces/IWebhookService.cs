namespace VirtualQueue.Application.Common.Interfaces;

public interface IWebhookService
{
    // Webhook Subscriptions (what controllers expect)
    Task<WebhookSubscriptionDto> CreateSubscriptionAsync(Guid tenantId, CreateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<WebhookSubscriptionDto?> GetSubscriptionAsync(Guid tenantId, Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<List<WebhookSubscriptionDto>> GetSubscriptionsByEventTypeAsync(Guid tenantId, string eventType, CancellationToken cancellationToken = default);
    Task<List<WebhookSubscriptionDto>> GetAllSubscriptionsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<WebhookSubscriptionDto> UpdateSubscriptionAsync(Guid tenantId, Guid subscriptionId, UpdateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteSubscriptionAsync(Guid tenantId, Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<bool> DispatchEventAsync(Guid tenantId, string eventType, object data, CancellationToken cancellationToken = default);
    
    // Original Webhook methods
    Task<WebhookDto> CreateWebhookAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default);
    Task<WebhookDto> UpdateWebhookAsync(Guid webhookId, UpdateWebhookRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteWebhookAsync(Guid webhookId, CancellationToken cancellationToken = default);
    Task<WebhookDto?> GetWebhookAsync(Guid webhookId, CancellationToken cancellationToken = default);
    Task<List<WebhookDto>> GetWebhooksAsync(Guid tenantId, WebhookEvent? eventType = null, CancellationToken cancellationToken = default);
    Task<bool> SendWebhookAsync(Guid webhookId, WebhookPayload payload, CancellationToken cancellationToken = default);
    Task<bool> SendWebhookAsync(string url, WebhookPayload payload, WebhookHeaders? headers = null, CancellationToken cancellationToken = default);
    Task<List<WebhookDelivery>> GetWebhookDeliveriesAsync(Guid webhookId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<bool> RetryWebhookDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken = default);
    Task<bool> TestWebhookAsync(Guid webhookId, CancellationToken cancellationToken = default);
}

public record WebhookDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Url,
    List<WebhookEvent> Events,
    WebhookHeaders? Headers,
    bool IsActive,
    int RetryCount,
    TimeSpan Timeout,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Description = null
);

public record CreateWebhookRequest(
    Guid TenantId,
    string Name,
    string Url,
    List<WebhookEvent> Events,
    WebhookHeaders? Headers = null,
    bool IsActive = true,
    int RetryCount = 3,
    TimeSpan? Timeout = null,
    string? Description = null
);

public record UpdateWebhookRequest(
    string? Name,
    string? Url,
    List<WebhookEvent>? Events,
    WebhookHeaders? Headers,
    bool? IsActive,
    int? RetryCount,
    TimeSpan? Timeout,
    string? Description
);

public record WebhookHeaders(
    Dictionary<string, string> Headers
);

public record WebhookPayload(
    WebhookEvent Event,
    Guid TenantId,
    object Data,
    DateTime Timestamp,
    string? Id = null
);

public record WebhookDelivery(
    Guid Id,
    Guid WebhookId,
    string Url,
    WebhookPayload Payload,
    int StatusCode,
    string? Response,
    string? Error,
    DateTime AttemptedAt,
    DateTime? DeliveredAt,
    bool IsSuccessful
);

public enum WebhookEvent
{
    UserEnqueued,
    UserReleased,
    UserDropped,
    UserServed,
    QueueCreated,
    QueueUpdated,
    QueueDeleted,
    QueueActivated,
    QueueDeactivated,
    TenantCreated,
    TenantUpdated,
    TenantDeleted
}

// Additional types needed by controllers
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

public record CreateWebhookSubscriptionRequest(
    string Name,
    string Url,
    string EventType,
    bool IsActive = true,
    string? Secret = null
);

public record UpdateWebhookSubscriptionRequest(
    string Name,
    string Url,
    string EventType,
    bool IsActive,
    string? Secret
);
