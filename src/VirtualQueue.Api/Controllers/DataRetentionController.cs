using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/data-retention")]
public class DataRetentionController : ControllerBase
{
    private readonly IDataRetentionService _retentionService;
    private readonly ILogger<DataRetentionController> _logger;

    public DataRetentionController(IDataRetentionService retentionService, ILogger<DataRetentionController> logger)
    {
        _retentionService = retentionService;
        _logger = logger;
    }

    [HttpPost("policies")]
    public async Task<ActionResult<RetentionPolicyDto>> CreateRetentionPolicy(Guid tenantId, [FromBody] CreateRetentionPolicyRequest request)
    {
        try
        {
            var policy = await _retentionService.CreateRetentionPolicyAsync(
                new VirtualQueue.Application.Common.Interfaces.CreateRetentionPolicyRequest(
                    tenantId,
                    request.EntityType,
                    $"Retention policy for {request.EntityType}",
                    Enum.Parse<VirtualQueue.Application.Common.Interfaces.RetentionEntityType>(request.EntityType),
                    TimeSpan.FromDays(request.RetentionDays),
                    VirtualQueue.Application.Common.Interfaces.RetentionAction.Delete,
                    null,
                    request.IsActive
                ));
            
            _logger.LogInformation("Retention policy created for tenant {TenantId}: {EntityType}", tenantId, request.EntityType);
            return CreatedAtAction(nameof(GetRetentionPolicy), new { tenantId, policyId = policy.Id }, policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating retention policy for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Retention policy creation error" });
        }
    }

    [HttpGet("policies/{policyId}")]
    public async Task<ActionResult<RetentionPolicyDto>> GetRetentionPolicy(Guid tenantId, Guid policyId)
    {
        try
        {
            var policy = await _retentionService.GetRetentionPolicyAsync(policyId);
            if (policy == null)
                return NotFound(new { message = "Retention policy not found" });
            
            return Ok(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving retention policy {PolicyId} for tenant {TenantId}", policyId, tenantId);
            return StatusCode(500, new { message = "Retention policy retrieval error" });
        }
    }

    [HttpGet("policies/by-entity/{entityType}")]
    public async Task<ActionResult<RetentionPolicyDto>> GetRetentionPolicyByEntityType(Guid tenantId, string entityType)
    {
        try
        {
            var policy = await _retentionService.GetRetentionPolicyByEntityTypeAsync(entityType);
            if (policy == null)
                return NotFound(new { message = "Retention policy not found" });
            
            return Ok(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving retention policy for entity {EntityType} and tenant {TenantId}", entityType, tenantId);
            return StatusCode(500, new { message = "Retention policy retrieval error" });
        }
    }

    [HttpGet("policies")]
    public async Task<ActionResult<List<RetentionPolicyDto>>> GetAllRetentionPolicies(Guid tenantId)
    {
        try
        {
            var policies = await _retentionService.GetAllRetentionPoliciesAsync(tenantId);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving retention policies for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Retention policies retrieval error" });
        }
    }

    [HttpPut("policies/{policyId}")]
    public async Task<ActionResult> UpdateRetentionPolicy(Guid tenantId, Guid policyId, [FromBody] UpdateRetentionPolicyRequest request)
    {
        try
        {
            await _retentionService.UpdateRetentionPolicyAsync(
                policyId,
                $"Policy for Unknown",
                $"Updated retention policy");
            
            _logger.LogInformation("Retention policy updated for tenant {TenantId}: {PolicyId}", tenantId, policyId);
            return Ok(new { message = "Retention policy updated successfully" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Retention policy not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating retention policy {PolicyId} for tenant {TenantId}", policyId, tenantId);
            return StatusCode(500, new { message = "Retention policy update error" });
        }
    }

    [HttpDelete("policies/{policyId}")]
    public async Task<ActionResult> DeleteRetentionPolicy(Guid tenantId, Guid policyId)
    {
        try
        {
            await _retentionService.DeleteRetentionPolicyAsync(policyId);
            _logger.LogInformation("Retention policy deleted for tenant {TenantId}: {PolicyId}", tenantId, policyId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting retention policy {PolicyId} for tenant {TenantId}", policyId, tenantId);
            return StatusCode(500, new { message = "Retention policy deletion error" });
        }
    }

    [HttpPost("apply/{entityType}")]
    public async Task<ActionResult> ApplyRetentionPolicy(Guid tenantId, string entityType)
    {
        try
        {
            var policy = await _retentionService.GetRetentionPolicyByEntityTypeAsync(entityType);
            if (policy == null)
                return NotFound(new { message = "Retention policy not found for entity type" });
            
            await _retentionService.ApplyRetentionPolicyAsync(policy.Id);
            _logger.LogInformation("Retention policy applied for tenant {TenantId}: {EntityType}", tenantId, entityType);
            return Ok(new { message = "Retention policy applied successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying retention policy for entity {EntityType} and tenant {TenantId}", entityType, tenantId);
            return StatusCode(500, new { message = "Retention policy application error" });
        }
    }

    [HttpPost("apply-all")]
    public async Task<ActionResult> ApplyAllRetentionPolicies(Guid tenantId)
    {
        try
        {
            await _retentionService.ApplyAllRetentionPoliciesAsync(tenantId);
            _logger.LogInformation("All retention policies applied for tenant {TenantId}", tenantId);
            return Ok(new { message = "All retention policies applied successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying all retention policies for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Retention policies application error" });
        }
    }
}

public record CreateRetentionPolicyRequest(string EntityType, int RetentionDays, bool IsActive);
public record UpdateRetentionPolicyRequest(int? RetentionDays, bool? IsActive);
