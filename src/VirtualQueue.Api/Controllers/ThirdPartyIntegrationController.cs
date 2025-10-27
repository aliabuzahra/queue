using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/integrations")]
public class ThirdPartyIntegrationController : ControllerBase
{
    private readonly IThirdPartyIntegrationService _integrationService;
    private readonly ILogger<ThirdPartyIntegrationController> _logger;

    public ThirdPartyIntegrationController(IThirdPartyIntegrationService integrationService, ILogger<ThirdPartyIntegrationController> logger)
    {
        _integrationService = integrationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<IntegrationDto>> CreateIntegration(Guid tenantId, [FromBody] CreateIntegrationRequest request)
    {
        try
        {
            var integration = await _integrationService.CreateIntegrationAsync(
                new VirtualQueue.Application.Common.Interfaces.CreateIntegrationRequest(
                    tenantId,
                    request.Name,
                    "", // No description available in API request
                    Enum.Parse<VirtualQueue.Application.Common.Interfaces.IntegrationType>(request.Type),
                    VirtualQueue.Application.Common.Interfaces.IntegrationProvider.Custom,
                    new Dictionary<string, object> { { "config", request.ConfigurationJson ?? "{}" } },
                    request.IsActive
                ));
            
            _logger.LogInformation("Third-party integration created for tenant {TenantId}: {IntegrationName}", tenantId, request.Name);
            return CreatedAtAction(nameof(GetIntegration), new { tenantId, integrationId = integration.Id }, integration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating third-party integration for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Integration creation error" });
        }
    }

    [HttpGet("{integrationId}")]
    public async Task<ActionResult<IntegrationDto>> GetIntegration(Guid tenantId, Guid integrationId)
    {
        try
        {
            var integration = await _integrationService.GetIntegrationAsync(tenantId, integrationId);
            if (integration == null)
                return NotFound(new { message = "Integration not found" });
            
            return Ok(integration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving integration {IntegrationId} for tenant {TenantId}", integrationId, tenantId);
            return StatusCode(500, new { message = "Integration retrieval error" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<IntegrationDto>>> GetAllIntegrations(Guid tenantId)
    {
        try
        {
            var integrations = await _integrationService.GetAllIntegrationsAsync(tenantId);
            return Ok(integrations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving integrations for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Integrations retrieval error" });
        }
    }

    [HttpPut("{integrationId}")]
    public async Task<ActionResult> UpdateIntegration(Guid tenantId, Guid integrationId, [FromBody] UpdateIntegrationRequest request)
    {
        try
        {
            await _integrationService.UpdateIntegrationAsync(
                tenantId,
                integrationId,
                request.Name,
                request.Type,
                request.ConfigurationJson);
            
            _logger.LogInformation("Third-party integration updated for tenant {TenantId}: {IntegrationId}", tenantId, integrationId);
            return Ok(new { message = "Integration updated successfully" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Integration not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating integration {IntegrationId} for tenant {TenantId}", integrationId, tenantId);
            return StatusCode(500, new { message = "Integration update error" });
        }
    }

    [HttpDelete("{integrationId}")]
    public async Task<ActionResult> DeleteIntegration(Guid tenantId, Guid integrationId)
    {
        try
        {
            await _integrationService.DeleteIntegrationAsync(tenantId, integrationId);
            _logger.LogInformation("Third-party integration deleted for tenant {TenantId}: {IntegrationId}", tenantId, integrationId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting integration {IntegrationId} for tenant {TenantId}", integrationId, tenantId);
            return StatusCode(500, new { message = "Integration deletion error" });
        }
    }

    [HttpPost("{integrationId}/test")]
    public async Task<ActionResult<IntegrationTestResponse>> TestIntegration(Guid tenantId, Guid integrationId)
    {
        try
        {
            var success = await _integrationService.TestIntegrationAsync(tenantId, integrationId);
            var response = new IntegrationTestResponse(success, DateTime.UtcNow);
            
            _logger.LogInformation("Integration test completed for tenant {TenantId}: {IntegrationId} - {Success}", 
                tenantId, integrationId, success);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing integration {IntegrationId} for tenant {TenantId}", integrationId, tenantId);
            return StatusCode(500, new { message = "Integration test error" });
        }
    }

    [HttpGet("{integrationId}/status")]
    public async Task<ActionResult<VirtualQueue.Application.Common.Interfaces.IntegrationStatusDto>> GetIntegrationStatus(Guid tenantId, Guid integrationId)
    {
        try
        {
            var status = await _integrationService.GetIntegrationStatusAsync(integrationId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving integration status {IntegrationId} for tenant {TenantId}", integrationId, tenantId);
            return StatusCode(500, new { message = "Integration status retrieval error" });
        }
    }

    [HttpPost("{integrationId}/dispatch")]
    public async Task<ActionResult> DispatchEvent(Guid tenantId, Guid integrationId, [FromBody] IntegrationDispatchEventRequest request)
    {
        try
        {
            await _integrationService.DispatchEventToIntegrationAsync(tenantId, integrationId, request.EventType, request.Payload);
            _logger.LogInformation("Event dispatched to integration {IntegrationId} for tenant {TenantId}: {EventType}", 
                integrationId, tenantId, request.EventType);
            return Ok(new { message = "Event dispatched successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching event to integration {IntegrationId} for tenant {TenantId}", integrationId, tenantId);
            return StatusCode(500, new { message = "Event dispatch error" });
        }
    }
}

public record CreateIntegrationRequest(
    string Name,
    string Type,
    string ConfigurationJson,
    bool IsActive);

public record UpdateIntegrationRequest(
    string? Name,
    string? Type,
    string? ConfigurationJson,
    bool? IsActive);

public record IntegrationTestResponse(bool Success, DateTime TestedAt);
public record IntegrationDispatchEventRequest(string EventType, object Payload);
