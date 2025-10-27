using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Commands.Queues;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.Queues;

namespace VirtualQueue.Api.Controllers;

/// <summary>
/// Controller for managing queue operations within a tenant.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId}/[controller]")]
[Produces("application/json")]
public class QueuesController : ControllerBase
{
    #region Fields
    private readonly IMediator _mediator;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="QueuesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the mediator is null.
    /// </exception>
    public QueuesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Creates a new queue for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="request">The queue creation request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The created queue information.
    /// </returns>
    /// <response code="201">Queue created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(QueueDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QueueDto>> CreateQueue(
        Guid tenantId, 
        [FromBody] CreateQueueRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant ID");
                
            if (request == null)
                return BadRequest("Request cannot be null");

            var command = new CreateQueueCommand(
                tenantId,
                request.Name,
                request.Description,
                request.MaxConcurrentUsers,
                request.ReleaseRatePerMinute);
                
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetQueues), new { tenantId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the queue");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QueueDto>>> GetQueues(Guid tenantId)
    {
        var query = new GetQueuesByTenantIdQuery(tenantId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{queueId}")]
    public async Task<ActionResult<QueueDto>> GetQueue(Guid tenantId, Guid queueId)
    {
        var query = new GetQueueByIdQuery(tenantId, queueId);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPut("{queueId}")]
    public async Task<ActionResult<QueueDto>> UpdateQueue(Guid tenantId, Guid queueId, [FromBody] UpdateQueueRequest request)
    {
        var command = new UpdateQueueCommand(
            tenantId,
            queueId,
            request.Name,
            request.Description,
            request.MaxConcurrentUsers,
            request.ReleaseRatePerMinute);
            
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{queueId}")]
    public async Task<ActionResult> DeleteQueue(Guid tenantId, Guid queueId)
    {
        var command = new DeleteQueueCommand(tenantId, queueId);
        var deleted = await _mediator.Send(command);
        
        if (!deleted)
            return NotFound();
            
        return NoContent();
    }

    [HttpPatch("{queueId}/activate")]
    public async Task<ActionResult<QueueDto>> ActivateQueue(Guid tenantId, Guid queueId)
    {
        var command = new ActivateQueueCommand(tenantId, queueId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("{queueId}/deactivate")]
    public async Task<ActionResult<QueueDto>> DeactivateQueue(Guid tenantId, Guid queueId)
    {
        var command = new DeactivateQueueCommand(tenantId, queueId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{queueId}/schedule")]
    public async Task<ActionResult<QueueDto>> SetQueueSchedule(
        Guid tenantId, 
        Guid queueId, 
        [FromBody] SetQueueScheduleRequest request)
    {
        var command = new SetQueueScheduleCommand(tenantId, queueId, request.Schedule);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{queueId}/availability")]
    public async Task<ActionResult<QueueAvailabilityDto>> GetQueueAvailability(
        Guid tenantId, 
        Guid queueId, 
        [FromQuery] DateTime? checkTime = null)
    {
        var query = new GetQueueAvailabilityQuery(tenantId, queueId, checkTime);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

/// <summary>
/// Request model for creating a new queue.
/// </summary>
/// <param name="Name">The name of the queue.</param>
/// <param name="Description">The description of the queue.</param>
/// <param name="MaxConcurrentUsers">The maximum number of concurrent users.</param>
/// <param name="ReleaseRatePerMinute">The rate at which users are released per minute.</param>
public record CreateQueueRequest(
    string Name,
    string Description,
    int MaxConcurrentUsers,
    int ReleaseRatePerMinute);

/// <summary>
/// Request model for updating an existing queue.
/// </summary>
/// <param name="Name">The name of the queue.</param>
/// <param name="Description">The description of the queue.</param>
/// <param name="MaxConcurrentUsers">The maximum number of concurrent users.</param>
/// <param name="ReleaseRatePerMinute">The rate at which users are released per minute.</param>
public record UpdateQueueRequest(
    string Name,
    string Description,
    int MaxConcurrentUsers,
    int ReleaseRatePerMinute);

/// <summary>
/// Request model for setting queue schedule.
/// </summary>
/// <param name="Schedule">The schedule configuration for the queue.</param>
public record SetQueueScheduleRequest(
    QueueScheduleDto Schedule
);

#endregion
