using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/alerts")]
public class AlertManagementController : ControllerBase
{
    private readonly IAlertManagementService _alertService;
    private readonly ILogger<AlertManagementController> _logger;

    public AlertManagementController(IAlertManagementService alertService, ILogger<AlertManagementController> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    [HttpPost("rules")]
    public async Task<ActionResult<VirtualQueue.Application.Common.Interfaces.AlertRuleDto>> CreateAlertRule(Guid tenantId, [FromBody] CreateAlertRuleRequest request)
    {
        try
        {
            var rule = await _alertService.CreateAlertRuleAsync(
                tenantId,
                request.Name,
                request.Description,
                request.Metric,
                request.Condition,
                request.Threshold,
                request.Severity,
                request.NotificationChannels != null ? string.Join(",", request.NotificationChannels) : null);
            
            _logger.LogInformation("Alert rule created for tenant {TenantId}: {RuleName}", tenantId, request.Name);
            return CreatedAtAction(nameof(GetAlertRule), new { tenantId, ruleId = rule.Id }, rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alert rule for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Alert rule creation error" });
        }
    }

    [HttpGet("rules/{ruleId}")]
    public async Task<ActionResult<VirtualQueue.Application.Common.Interfaces.AlertRuleDto>> GetAlertRule(Guid tenantId, Guid ruleId)
    {
        try
        {
            var rule = await _alertService.GetAlertRuleAsync(tenantId, ruleId);
            if (rule == null)
                return NotFound(new { message = "Alert rule not found" });
            
            return Ok(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return StatusCode(500, new { message = "Alert rule retrieval error" });
        }
    }

    [HttpGet("rules")]
    public async Task<ActionResult<List<VirtualQueue.Application.Common.Interfaces.AlertRuleDto>>> GetAllAlertRules(Guid tenantId)
    {
        try
        {
            var rules = await _alertService.GetAllAlertRulesAsync(tenantId);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert rules for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Alert rules retrieval error" });
        }
    }

    [HttpPut("rules/{ruleId}")]
    public async Task<ActionResult> UpdateAlertRule(Guid tenantId, Guid ruleId, [FromBody] UpdateAlertRuleRequest request)
    {
        try
        {
            await _alertService.UpdateAlertRuleAsync(
                tenantId,
                ruleId,
                request.Name,
                request.Description,
                request.Metric,
                request.Condition,
                request.Threshold ?? 0.0,
                request.Severity,
                request.NotificationChannels != null ? string.Join(",", request.NotificationChannels) : null);
            
            _logger.LogInformation("Alert rule updated for tenant {TenantId}: {RuleId}", tenantId, ruleId);
            return Ok(new { message = "Alert rule updated successfully" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Alert rule not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alert rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return StatusCode(500, new { message = "Alert rule update error" });
        }
    }

    [HttpDelete("rules/{ruleId}")]
    public async Task<ActionResult> DeleteAlertRule(Guid tenantId, Guid ruleId)
    {
        try
        {
            await _alertService.DeleteAlertRuleAsync(tenantId, ruleId);
            _logger.LogInformation("Alert rule deleted for tenant {TenantId}: {RuleId}", tenantId, ruleId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting alert rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return StatusCode(500, new { message = "Alert rule deletion error" });
        }
    }

    [HttpPost("trigger")]
    public async Task<ActionResult> TriggerAlert(Guid tenantId, [FromBody] TriggerAlertRequest request)
    {
        try
        {
            await _alertService.TriggerAlertAsync(tenantId, request.RuleId, request.Message);
            _logger.LogInformation("Alert triggered for tenant {TenantId}: {RuleId}", tenantId, request.RuleId);
            return Ok(new { message = "Alert triggered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering alert {RuleId} for tenant {TenantId}", request.RuleId, tenantId);
            return StatusCode(500, new { message = "Alert trigger error" });
        }
    }

    [HttpPost("resolve/{alertId}")]
    public async Task<ActionResult> ResolveAlert(Guid tenantId, Guid alertId, [FromBody] ResolveAlertRequest? request = null)
    {
        try
        {
            await _alertService.ResolveAlertAsync(tenantId, alertId, request?.ResolutionNotes);
            _logger.LogInformation("Alert resolved for tenant {TenantId}: {AlertId}", tenantId, alertId);
            return Ok(new { message = "Alert resolved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert {AlertId} for tenant {TenantId}", alertId, tenantId);
            return StatusCode(500, new { message = "Alert resolution error" });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<VirtualQueue.Application.DTOs.AlertDto>>> GetActiveAlerts(Guid tenantId)
    {
        try
        {
            var alerts = await _alertService.GetActiveAlertsAsync(tenantId);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active alerts for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Active alerts retrieval error" });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<VirtualQueue.Application.DTOs.AlertDto>>> GetAlertHistory(
        Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var alerts = await _alertService.GetAlertHistoryAsync(tenantId, startDate, endDate);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert history for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Alert history retrieval error" });
        }
    }
}

public record CreateAlertRuleRequest(
    string Name,
    string Description,
    string Metric,
    string Condition,
    double Threshold,
    string Severity,
    List<string> NotificationChannels);

public record UpdateAlertRuleRequest(
    string? Name,
    string? Description,
    string? Metric,
    string? Condition,
    double? Threshold,
    string? Severity,
    List<string>? NotificationChannels,
    bool? IsActive);

public record TriggerAlertRequest(Guid RuleId, string Message, Dictionary<string, string>? Details);
public record ResolveAlertRequest(string? ResolutionNotes);
