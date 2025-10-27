using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/load-balancing")]
public class LoadBalancingController : ControllerBase
{
    private readonly IQueueLoadBalancingService _loadBalancingService;
    private readonly ILogger<LoadBalancingController> _logger;

    public LoadBalancingController(IQueueLoadBalancingService loadBalancingService, ILogger<LoadBalancingController> logger)
    {
        _loadBalancingService = loadBalancingService;
        _logger = logger;
    }

    [HttpGet("status/{queueId}")]
    public async Task<ActionResult<QueueLoadStatus>> GetQueueLoadStatus(Guid tenantId, Guid queueId)
    {
        try
        {
            var status = await _loadBalancingService.GetQueueLoadStatusAsync(queueId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving load status for queue {QueueId} and tenant {TenantId}", queueId, tenantId);
            return StatusCode(500, new { message = "Load status retrieval error" });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<List<QueueLoadStatus>>> GetAllQueueLoadStatuses(Guid tenantId)
    {
        try
        {
            var statuses = await _loadBalancingService.GetAllQueueLoadStatusesAsync(tenantId);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving load statuses for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Load statuses retrieval error" });
        }
    }

    [HttpPost("optimal-queue")]
    public async Task<ActionResult<OptimalQueueResponse>> GetOptimalQueue(Guid tenantId, [FromBody] GetOptimalQueueRequest request)
    {
        try
        {
            var optimalQueueId = await _loadBalancingService.GetOptimalQueueAsync(tenantId);
            var response = new OptimalQueueResponse(optimalQueueId.QueueId, DateTime.UtcNow);
            
            _logger.LogInformation("Optimal queue found for tenant {TenantId}: {QueueId}", tenantId, optimalQueueId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding optimal queue for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Optimal queue selection error" });
        }
    }

    [HttpPost("rebalance")]
    public async Task<ActionResult> RebalanceQueue(Guid tenantId, [FromBody] RebalanceQueueRequest request)
    {
        try
        {
            var success = await _loadBalancingService.RebalanceQueueAsync(request.SourceQueueId);
            
            if (success)
            {
                _logger.LogInformation("Queue rebalanced for tenant {TenantId}: {SourceQueueId} -> {DestinationQueueId}", 
                    tenantId, request.SourceQueueId, request.DestinationQueueId);
                return Ok(new { message = "Queue rebalanced successfully" });
            }
            else
            {
                return BadRequest(new { message = "Queue rebalancing failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebalancing queue for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Queue rebalancing error" });
        }
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<LoadBalancingRecommendation>> GetLoadBalancingRecommendations(Guid tenantId)
    {
        try
        {
            var recommendations = await _loadBalancingService.GetLoadBalancingRecommendationsAsync(tenantId);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving load balancing recommendations for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Recommendations retrieval error" });
        }
    }

    [HttpPost("auto-scaling/enable")]
    public async Task<ActionResult> EnableAutoScaling(Guid tenantId, [FromBody] EnableAutoScalingRequest request)
    {
        try
        {
            var success = await _loadBalancingService.EnableAutoScalingAsync(request.QueueId, request.Config);
            
            if (success)
            {
                _logger.LogInformation("Auto-scaling enabled for queue {QueueId} and tenant {TenantId}", request.QueueId, tenantId);
                return Ok(new { message = "Auto-scaling enabled successfully" });
            }
            else
            {
                return BadRequest(new { message = "Auto-scaling enablement failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling auto-scaling for queue {QueueId} and tenant {TenantId}", request.QueueId, tenantId);
            return StatusCode(500, new { message = "Auto-scaling enablement error" });
        }
    }

    [HttpPost("auto-scaling/disable")]
    public async Task<ActionResult> DisableAutoScaling(Guid tenantId, [FromBody] DisableAutoScalingRequest request)
    {
        try
        {
            var success = await _loadBalancingService.DisableAutoScalingAsync(request.QueueId);
            
            if (success)
            {
                _logger.LogInformation("Auto-scaling disabled for queue {QueueId} and tenant {TenantId}", request.QueueId, tenantId);
                return Ok(new { message = "Auto-scaling disabled successfully" });
            }
            else
            {
                return BadRequest(new { message = "Auto-scaling disablement failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling auto-scaling for queue {QueueId} and tenant {TenantId}", request.QueueId, tenantId);
            return StatusCode(500, new { message = "Auto-scaling disablement error" });
        }
    }
}

public record GetOptimalQueueRequest(string UserIdentifier);
public record OptimalQueueResponse(Guid QueueId, DateTime SelectedAt);
public record RebalanceQueueRequest(Guid SourceQueueId, Guid DestinationQueueId, int NumberOfUsers);
public record EnableAutoScalingRequest(Guid QueueId, AutoScalingConfig Config);
public record DisableAutoScalingRequest(Guid QueueId);
