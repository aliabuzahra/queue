using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<VirtualQueue.Application.Common.Interfaces.WebhookSubscriptionDto>> CreateWebhook(Guid tenantId, [FromBody] CreateWebhookRequest request)
    {
        try
        {
            var webhook = await _webhookService.CreateSubscriptionAsync(
                tenantId,
                new VirtualQueue.Application.Common.Interfaces.CreateWebhookSubscriptionRequest(
                    request.EventType, // Name
                    request.CallbackUrl, // Url
                    request.EventType, // EventType
                    true, // IsActive
                    request.Secret // Secret
                ));
            
            _logger.LogInformation("Webhook created for tenant {TenantId}: {EventType} -> {CallbackUrl}", 
                tenantId, request.EventType, request.CallbackUrl);
            return CreatedAtAction(nameof(GetWebhook), new { tenantId, subscriptionId = webhook.Id }, webhook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating webhook for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Webhook creation error" });
        }
    }

    [HttpGet("{subscriptionId}")]
    public async Task<ActionResult<VirtualQueue.Application.Common.Interfaces.WebhookSubscriptionDto>> GetWebhook(Guid tenantId, Guid subscriptionId)
    {
        try
        {
            var webhook = await _webhookService.GetSubscriptionAsync(tenantId, subscriptionId);
            if (webhook == null)
                return NotFound(new { message = "Webhook subscription not found" });
            
            return Ok(webhook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhook {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return StatusCode(500, new { message = "Webhook retrieval error" });
        }
    }

    [HttpGet("by-event/{eventType}")]
    public async Task<ActionResult<List<VirtualQueue.Application.Common.Interfaces.WebhookSubscriptionDto>>> GetWebhooksByEventType(Guid tenantId, string eventType)
    {
        try
        {
            var webhooks = await _webhookService.GetSubscriptionsByEventTypeAsync(tenantId, eventType);
            return Ok(webhooks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhooks for event {EventType} and tenant {TenantId}", eventType, tenantId);
            return StatusCode(500, new { message = "Webhooks retrieval error" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<VirtualQueue.Application.Common.Interfaces.WebhookSubscriptionDto>>> GetAllWebhooks(Guid tenantId)
    {
        try
        {
            var webhooks = await _webhookService.GetAllSubscriptionsAsync(tenantId);
            return Ok(webhooks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhooks for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Webhooks retrieval error" });
        }
    }

    [HttpPut("{subscriptionId}")]
    public async Task<ActionResult> UpdateWebhook(Guid tenantId, Guid subscriptionId, [FromBody] UpdateWebhookRequest request)
    {
        try
        {
            await _webhookService.UpdateSubscriptionAsync(
                tenantId,
                subscriptionId,
                new VirtualQueue.Application.Common.Interfaces.UpdateWebhookSubscriptionRequest(
                    request.EventType ?? "", // Name
                    request.CallbackUrl ?? "", // Url
                    request.EventType ?? "", // EventType
                    request.IsActive ?? true, // IsActive
                    request.Secret // Secret
                ));
            
            _logger.LogInformation("Webhook updated for tenant {TenantId}: {SubscriptionId}", tenantId, subscriptionId);
            return Ok(new { message = "Webhook updated successfully" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Webhook subscription not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating webhook {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return StatusCode(500, new { message = "Webhook update error" });
        }
    }

    [HttpDelete("{subscriptionId}")]
    public async Task<ActionResult> DeleteWebhook(Guid tenantId, Guid subscriptionId)
    {
        try
        {
            await _webhookService.DeleteSubscriptionAsync(tenantId, subscriptionId);
            _logger.LogInformation("Webhook deleted for tenant {TenantId}: {SubscriptionId}", tenantId, subscriptionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);
            return StatusCode(500, new { message = "Webhook deletion error" });
        }
    }

    [HttpPost("dispatch")]
    public async Task<ActionResult> DispatchEvent(Guid tenantId, [FromBody] DispatchEventRequest request)
    {
        try
        {
            await _webhookService.DispatchEventAsync(tenantId, request.EventType, request.Payload);
            _logger.LogInformation("Event dispatched for tenant {TenantId}: {EventType}", tenantId, request.EventType);
            return Ok(new { message = "Event dispatched successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching event {EventType} for tenant {TenantId}", request.EventType, tenantId);
            return StatusCode(500, new { message = "Event dispatch error" });
        }
    }

    #region Request/Response Models
    public record CreateWebhookRequest(string EventType, string CallbackUrl, string? Secret);
    public record UpdateWebhookRequest(string? EventType, string? CallbackUrl, string? Secret, bool? IsActive);
    public record DispatchEventRequest(string EventType, object Payload);
    #endregion
}
