using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/business-rules")]
public class BusinessRulesController : ControllerBase
{
    private readonly IBusinessRulesEngine _rulesEngine;
    private readonly ILogger<BusinessRulesController> _logger;

    public BusinessRulesController(IBusinessRulesEngine rulesEngine, ILogger<BusinessRulesController> logger)
    {
        _rulesEngine = rulesEngine;
        _logger = logger;
    }

    [HttpPost("rules")]
    public async Task<ActionResult<RuleDto>> CreateRule(Guid tenantId, [FromBody] CreateRuleRequest request)
    {
        try
        {
            var rule = await _rulesEngine.CreateRuleAsync(
                tenantId,
                request.Name,
                request.Description,
                request.Type,
                request.Condition,
                request.Action,
                request.Priority,
                request.IsActive);
            
            _logger.LogInformation("Business rule created for tenant {TenantId}: {RuleName}", tenantId, request.Name);
            return CreatedAtAction(nameof(GetRule), new { tenantId, ruleId = rule.Id }, rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business rule for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Business rule creation error" });
        }
    }

    [HttpGet("rules/{ruleId}")]
    public async Task<ActionResult<RuleDto>> GetRule(Guid tenantId, Guid ruleId)
    {
        try
        {
            var rule = await _rulesEngine.GetRuleAsync(ruleId);
            if (rule == null)
                return NotFound(new { message = "Business rule not found" });
            
            return Ok(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return StatusCode(500, new { message = "Business rule retrieval error" });
        }
    }

    [HttpGet("rules")]
    public async Task<ActionResult<List<RuleDto>>> GetAllRules(Guid tenantId)
    {
        try
        {
            var rules = await _rulesEngine.GetAllRulesAsync(tenantId);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business rules for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Business rules retrieval error" });
        }
    }

    [HttpGet("rules/by-type/{type}")]
    public async Task<ActionResult<List<RuleDto>>> GetRulesByType(Guid tenantId, string type)
    {
        try
        {
            if (!Enum.TryParse<RuleType>(type, true, out var ruleType))
                return BadRequest(new { message = "Invalid rule type" });
            
            var rules = await _rulesEngine.GetRulesByTypeAsync(type);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business rules by type {Type} for tenant {TenantId}", type, tenantId);
            return StatusCode(500, new { message = "Business rules retrieval error" });
        }
    }

    [HttpPut("rules/{ruleId}")]
    public async Task<ActionResult> UpdateRule(Guid tenantId, Guid ruleId, [FromBody] UpdateRuleRequest request)
    {
        try
        {
            await _rulesEngine.UpdateRuleAsync(
                ruleId,
                request.Name,
                request.Description,
                request.Type,
                request.Condition,
                request.Action,
                request.Priority ?? 0,
                request.IsActive ?? true);
            
            _logger.LogInformation("Business rule updated for tenant {TenantId}: {RuleId}", tenantId, ruleId);
            return Ok(new { message = "Business rule updated successfully" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Business rule not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return StatusCode(500, new { message = "Business rule update error" });
        }
    }

    [HttpDelete("rules/{ruleId}")]
    public async Task<ActionResult> DeleteRule(Guid tenantId, Guid ruleId)
    {
        try
        {
            await _rulesEngine.DeleteRuleAsync(ruleId);
            _logger.LogInformation("Business rule deleted for tenant {TenantId}: {RuleId}", tenantId, ruleId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business rule {RuleId} for tenant {TenantId}", ruleId, tenantId);
            return StatusCode(500, new { message = "Business rule deletion error" });
        }
    }

    [HttpPost("execute")]
    public async Task<ActionResult<RuleExecutionResult>> ExecuteRules(Guid tenantId, [FromBody] ExecuteRulesRequest request)
    {
        try
        {
            var context = new RuleExecutionContext(
                tenantId,
                request.QueueId,
                request.UserSessionId?.ToString(),
                request.Variables,
                DateTime.UtcNow);
            
            var result = await _rulesEngine.ExecuteRulesAsync(tenantId, context);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing business rules for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Business rules execution error" });
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<RuleValidationResponse>> ValidateRule(Guid tenantId, [FromBody] ValidateRuleRequest request)
    {
        try
        {
            var isValid = await _rulesEngine.ValidateRuleAsync(request.Condition);
            var response = new RuleValidationResponse(isValid, DateTime.UtcNow);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating business rule for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Business rule validation error" });
        }
    }
}

public record CreateRuleRequest(
    string Name,
    string Description,
    string Type,
    string Condition,
    string Action,
    int Priority,
    bool IsActive);

public record UpdateRuleRequest(
    string? Name,
    string? Description,
    string? Type,
    string? Condition,
    string? Action,
    int? Priority,
    bool? IsActive);

public record ExecuteRulesRequest(
    Guid? QueueId,
    Guid? UserSessionId,
    Dictionary<string, object> Variables);

public record ValidateRuleRequest(string Condition, string Action);
public record RuleValidationResponse(bool IsValid, DateTime ValidatedAt);
