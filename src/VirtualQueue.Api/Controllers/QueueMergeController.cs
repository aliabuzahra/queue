using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Commands.Queues;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/queue-merge")]
public class QueueMergeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<QueueMergeController> _logger;

    public QueueMergeController(IMediator mediator, ILogger<QueueMergeController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("merge")]
    public async Task<ActionResult<QueueMergeOperationResultDto>> MergeQueues(
        Guid tenantId, 
        [FromBody] MergeQueuesRequest request)
    {
        try
        {
            var command = new MergeQueuesCommand(
                tenantId,
                request.SourceQueueId,
                request.DestinationQueueId,
                request.MaxUsersToMove);

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Queue merge completed for tenant {TenantId}: {Success}, {UsersMoved} users moved", 
                tenantId, result.Success, result.UsersMoved);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging queues for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Queue merge error" });
        }
    }

    [HttpPost("split")]
    public Task<ActionResult<QueueMergeOperationResultDto>> SplitQueue(
        Guid tenantId, 
        [FromBody] SplitQueueRequest request)
    {
        try
        {
            _logger.LogInformation("Splitting queue {SourceQueueId} for tenant {TenantId}", request.SourceQueueId, tenantId);
            
            // Mock implementation
            var result = new QueueMergeOperationResultDto(
                true,
                request.UsersToMove,
                $"Queue split successfully. {request.UsersToMove} users moved to new queue.",
                TimeSpan.FromMinutes(2)
            );
            
            return Task.FromResult<ActionResult<QueueMergeOperationResultDto>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error splitting queue for tenant {TenantId}", tenantId);
            return Task.FromResult<ActionResult<QueueMergeOperationResultDto>>(StatusCode(500, new { message = "Queue split error" }));
        }
    }

    [HttpPost("rebalance")]
    public Task<ActionResult<QueueMergeOperationResultDto>> RebalanceQueues(
        Guid tenantId, 
        [FromBody] RebalanceQueuesRequest request)
    {
        try
        {
            _logger.LogInformation("Rebalancing queues for tenant {TenantId}", tenantId);
            
            // Mock implementation
            var result = new QueueMergeOperationResultDto(
                true,
                Random.Shared.Next(10, 50),
                "Queues rebalanced successfully",
                TimeSpan.FromMinutes(5)
            );
            
            return Task.FromResult<ActionResult<QueueMergeOperationResultDto>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebalancing queues for tenant {TenantId}", tenantId);
            return Task.FromResult<ActionResult<QueueMergeOperationResultDto>>(StatusCode(500, new { message = "Queue rebalancing error" }));
        }
    }

    [HttpGet("operations")]
    public Task<ActionResult<List<QueueMergeOperationDto>>> GetMergeOperations(Guid tenantId)
    {
        try
        {
            _logger.LogInformation("Getting merge operations for tenant {TenantId}", tenantId);
            
            // Mock implementation
            var operations = new List<QueueMergeOperationDto>
            {
                new QueueMergeOperationDto(
                    Guid.NewGuid(),
                    tenantId,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Merge",
                    "Completed",
                    25,
                    25,
                    DateTime.UtcNow.AddHours(-2),
                    DateTime.UtcNow.AddHours(-1),
                    null,
                    new Dictionary<string, string>(),
                    DateTime.UtcNow.AddHours(-2)
                ),
                new QueueMergeOperationDto(
                    Guid.NewGuid(),
                    tenantId,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Split",
                    "InProgress",
                    50,
                    30,
                    DateTime.UtcNow.AddMinutes(-30),
                    null,
                    null,
                    new Dictionary<string, string>(),
                    DateTime.UtcNow.AddMinutes(-30)
                )
            };
            
            return Task.FromResult<ActionResult<List<QueueMergeOperationDto>>>(Ok(operations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merge operations for tenant {TenantId}", tenantId);
            return Task.FromResult<ActionResult<List<QueueMergeOperationDto>>>(StatusCode(500, new { message = "Merge operations retrieval error" }));
        }
    }

    [HttpGet("operations/{operationId}")]
    public Task<ActionResult<QueueMergeOperationDto>> GetMergeOperation(Guid tenantId, Guid operationId)
    {
        try
        {
            _logger.LogInformation("Getting merge operation {OperationId} for tenant {TenantId}", operationId, tenantId);
            
            // Mock implementation
            var operation = new QueueMergeOperationDto(
                operationId,
                tenantId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Merge",
                "Completed",
                25,
                25,
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1),
                null,
                new Dictionary<string, string>(),
                DateTime.UtcNow.AddHours(-2)
            );
            
            return Task.FromResult<ActionResult<QueueMergeOperationDto>>(Ok(operation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merge operation {OperationId} for tenant {TenantId}", operationId, tenantId);
            return Task.FromResult<ActionResult<QueueMergeOperationDto>>(StatusCode(500, new { message = "Merge operation retrieval error" }));
        }
    }
}

public record MergeQueuesRequest(Guid SourceQueueId, Guid DestinationQueueId, int? MaxUsersToMove = null);
public record SplitQueueRequest(Guid SourceQueueId, int UsersToMove, string NewQueueName);
public record RebalanceQueuesRequest(List<Guid> QueueIds);

