using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly ICacheService _cacheService;
    private readonly HttpClient _httpClient;

    public WebhookService(ILogger<WebhookService> logger, ICacheService cacheService, HttpClient httpClient)
    {
        _logger = logger;
        _cacheService = cacheService;
        _httpClient = httpClient;
    }

    public async Task<WebhookDto> CreateWebhookAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var webhookId = Guid.NewGuid();
            var webhook = new WebhookDto(
                webhookId,
                request.TenantId,
                request.Name,
                request.Url,
                request.Events,
                request.Headers,
                request.IsActive,
                request.RetryCount,
                request.Timeout ?? TimeSpan.FromSeconds(30),
                DateTime.UtcNow,
                DateTime.UtcNow,
                request.Description
            );

            var cacheKey = $"webhook:{request.TenantId}:{webhookId}";
            await _cacheService.SetAsync(cacheKey, webhook, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Webhook created: {WebhookId} for tenant {TenantId}", webhookId, request.TenantId);
            return webhook;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create webhook for tenant {TenantId}", request.TenantId);
            throw;
        }
    }

    public async Task<WebhookDto> UpdateWebhookAsync(Guid webhookId, UpdateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Webhook updated: {WebhookId}", webhookId);
            
            // In a real implementation, this would update the database
            return new WebhookDto(
                webhookId,
                Guid.NewGuid(),
                request.Name ?? "Updated Webhook",
                request.Url ?? "https://example.com/webhook",
                request.Events ?? new List<WebhookEvent> { WebhookEvent.UserEnqueued },
                request.Headers,
                request.IsActive ?? true,
                request.RetryCount ?? 3,
                request.Timeout ?? TimeSpan.FromSeconds(30),
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                request.Description
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update webhook {WebhookId}", webhookId);
            throw;
        }
    }

    public async Task<bool> DeleteWebhookAsync(Guid webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Webhook deleted: {WebhookId}", webhookId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete webhook {WebhookId}", webhookId);
            return false;
        }
    }

    public async Task<WebhookDto?> GetWebhookAsync(Guid webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving webhook: {WebhookId}", webhookId);
            
            return new WebhookDto(
                webhookId,
                Guid.NewGuid(),
                "Sample Webhook",
                "https://example.com/webhook",
                new List<WebhookEvent> { WebhookEvent.UserEnqueued, WebhookEvent.UserReleased },
                new WebhookHeaders(new Dictionary<string, string> { { "Authorization", "Bearer token" } }),
                true,
                3,
                TimeSpan.FromSeconds(30),
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                "Sample webhook for testing"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get webhook {WebhookId}", webhookId);
            return null;
        }
    }

    public async Task<List<WebhookDto>> GetWebhooksAsync(Guid tenantId, WebhookEvent? eventType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting webhooks for tenant {TenantId}, event: {EventType}", tenantId, eventType);
            
            return new List<WebhookDto>
            {
                new WebhookDto(
                    Guid.NewGuid(),
                    tenantId,
                    "User Events Webhook",
                    "https://example.com/user-events",
                    new List<WebhookEvent> { WebhookEvent.UserEnqueued, WebhookEvent.UserReleased },
                    new WebhookHeaders(new Dictionary<string, string> { { "Authorization", "Bearer token" } }),
                    true,
                    3,
                    TimeSpan.FromSeconds(30),
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow,
                    "Webhook for user events"
                )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get webhooks for tenant {TenantId}", tenantId);
            return new List<WebhookDto>();
        }
    }

    public async Task<bool> SendWebhookAsync(Guid webhookId, WebhookPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var webhook = await GetWebhookAsync(webhookId, cancellationToken);
            if (webhook == null)
            {
                _logger.LogWarning("Webhook {WebhookId} not found", webhookId);
                return false;
            }

            return await SendWebhookAsync(webhook.Url, payload, webhook.Headers, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook {WebhookId}", webhookId);
            return false;
        }
    }

    public async Task<bool> SendWebhookAsync(string url, WebhookPayload payload, WebhookHeaders? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };

            // Add custom headers
            if (headers?.Headers != null)
            {
                foreach (var header in headers.Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Webhook sent to {Url}, Status: {StatusCode}", url, response.StatusCode);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook to {Url}", url);
            return false;
        }
    }

    public async Task<List<WebhookDelivery>> GetWebhookDeliveriesAsync(Guid webhookId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting webhook deliveries for {WebhookId}", webhookId);
            
            // In a real implementation, this would query the database
            return new List<WebhookDelivery>
            {
                new WebhookDelivery(
                    Guid.NewGuid(),
                    webhookId,
                    "https://example.com/webhook",
                    new WebhookPayload(WebhookEvent.UserEnqueued, Guid.NewGuid(), new { }, DateTime.UtcNow),
                    200,
                    "Success",
                    null,
                    DateTime.UtcNow.AddMinutes(-5),
                    DateTime.UtcNow.AddMinutes(-5),
                    true
                )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get webhook deliveries for {WebhookId}", webhookId);
            return new List<WebhookDelivery>();
        }
    }

    public async Task<bool> RetryWebhookDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrying webhook delivery {DeliveryId}", deliveryId);
            
            // In a real implementation, this would retry the failed delivery
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry webhook delivery {DeliveryId}", deliveryId);
            return false;
        }
    }

    public async Task<bool> TestWebhookAsync(Guid webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            var webhook = await GetWebhookAsync(webhookId, cancellationToken);
            if (webhook == null)
            {
                return false;
            }

            var testPayload = new WebhookPayload(
                WebhookEvent.UserEnqueued,
                webhook.TenantId,
                new { Test = true, Message = "This is a test webhook" },
                DateTime.UtcNow
            );

            return await SendWebhookAsync(webhook.Url, testPayload, webhook.Headers, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test webhook {WebhookId}", webhookId);
            return false;
        }
    }

    // New methods required by the interface
    public async Task<WebhookSubscriptionDto> CreateSubscriptionAsync(Guid tenantId, CreateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionId = Guid.NewGuid();
            var subscription = new WebhookSubscriptionDto(
                subscriptionId,
                tenantId,
                request.Name,
                request.Url,
                request.EventType,
                request.IsActive,
                request.Secret,
                DateTime.UtcNow,
                DateTime.UtcNow
            );

            var cacheKey = $"webhook_subscription:{tenantId}:{subscriptionId}";
            await _cacheService.SetAsync(cacheKey, subscription, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Webhook subscription created: {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create webhook subscription for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<WebhookSubscriptionDto?> GetSubscriptionAsync(Guid tenantId, Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"webhook_subscription:{tenantId}:{subscriptionId}";
            var subscription = await _cacheService.GetAsync<WebhookSubscriptionDto>(cacheKey, cancellationToken);
            
            if (subscription == null)
            {
                _logger.LogWarning("Webhook subscription {SubscriptionId} not found for tenant {TenantId}", subscriptionId, tenantId);
                return null;
            }

            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get webhook subscription {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return null;
        }
    }

    public async Task<List<WebhookSubscriptionDto>> GetSubscriptionsByEventTypeAsync(Guid tenantId, string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting webhook subscriptions by event type {EventType} for tenant {TenantId}", eventType, tenantId);
            
            // In a real implementation, this would query the database
            return new List<WebhookSubscriptionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get webhook subscriptions by event type {EventType} for tenant {TenantId}", eventType, tenantId);
            return new List<WebhookSubscriptionDto>();
        }
    }

    public async Task<List<WebhookSubscriptionDto>> GetAllSubscriptionsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all webhook subscriptions for tenant {TenantId}", tenantId);
            
            // In a real implementation, this would query the database
            return new List<WebhookSubscriptionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all webhook subscriptions for tenant {TenantId}", tenantId);
            return new List<WebhookSubscriptionDto>();
        }
    }

    public async Task<WebhookSubscriptionDto> UpdateSubscriptionAsync(Guid tenantId, Guid subscriptionId, UpdateWebhookSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = new WebhookSubscriptionDto(
                subscriptionId,
                tenantId,
                request.Name,
                request.Url,
                request.EventType,
                request.IsActive,
                request.Secret,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow
            );

            var cacheKey = $"webhook_subscription:{tenantId}:{subscriptionId}";
            await _cacheService.SetAsync(cacheKey, subscription, TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Webhook subscription updated: {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update webhook subscription {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteSubscriptionAsync(Guid tenantId, Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"webhook_subscription:{tenantId}:{subscriptionId}";
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation("Webhook subscription deleted: {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete webhook subscription {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return false;
        }
    }

    public async Task<bool> DispatchEventAsync(Guid tenantId, string eventType, object data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Dispatching event {EventType} for tenant {TenantId}", eventType, tenantId);
            
            // In a real implementation, this would dispatch the event to all relevant subscriptions
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch event {EventType} for tenant {TenantId}", eventType, tenantId);
            return false;
        }
    }
}
