using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Commands.UserSessions;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.UserSessions;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Api.Controllers;

/// <summary>
/// Controller for managing user sessions within queues.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId}/queues/{queueId}/[controller]")]
[Produces("application/json")]
public class UserSessionsController : ControllerBase
{
    #region Fields
    private readonly IMediator _mediator;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSessionsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the mediator is null.
    /// </exception>
    public UserSessionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Enqueues a user in the specified queue.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="queueId">The queue identifier.</param>
    /// <param name="request">The enqueue user request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The created user session information.
    /// </returns>
    /// <response code="201">User enqueued successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("enqueue")]
    [ProducesResponseType(typeof(UserSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserSessionDto>> EnqueueUser(
        Guid tenantId, 
        Guid queueId, 
        [FromBody] EnqueueUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (tenantId == Guid.Empty)
                return BadRequest("Invalid tenant ID");
                
            if (queueId == Guid.Empty)
                return BadRequest("Invalid queue ID");
                
            if (request == null)
                return BadRequest("Request cannot be null");

            var command = new EnqueueUserCommand(
                tenantId,
                queueId,
                request.UserIdentifier,
                request.Metadata,
                request.Priority);
                
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetUserStatus), new { tenantId, queueId, userId = request.UserIdentifier }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while enqueuing the user");
        }
    }

    [HttpGet("status/{userId}")]
    public async Task<ActionResult<QueueStatusDto>> GetUserStatus(Guid tenantId, Guid queueId, string userId)
    {
        var query = new GetUserQueueStatusQuery(tenantId, queueId, userId);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost("release")]
    public async Task<ActionResult<ReleaseUsersResponse>> ReleaseUsers(
        Guid tenantId, 
        Guid queueId, 
        [FromBody] ReleaseUsersRequest request)
    {
        var command = new ReleaseUsersCommand(tenantId, queueId, request.Count);
        var releasedCount = await _mediator.Send(command);
        
        return Ok(new ReleaseUsersResponse(releasedCount));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetUserSessions(Guid tenantId, Guid queueId)
    {
        var query = new GetUserSessionsByQueueIdQuery(tenantId, queueId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult> RemoveUser(Guid tenantId, Guid queueId, string userId)
    {
        var command = new RemoveUserCommand(tenantId, queueId, userId);
        var removed = await _mediator.Send(command);
        
        if (!removed)
            return NotFound();
            
        return NoContent();
    }

    [HttpPatch("{userId}/drop")]
    public async Task<ActionResult<UserSessionDto>> DropUser(Guid tenantId, Guid queueId, string userId)
    {
        var command = new DropUserCommand(tenantId, queueId, userId);
        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
}

/// <summary>
/// Request model for enqueuing a user.
/// </summary>
/// <param name="UserIdentifier">The user identifier.</param>
/// <param name="Metadata">Optional metadata for the session.</param>
/// <param name="Priority">The priority level for the session.</param>
public record EnqueueUserRequest(string UserIdentifier, string? Metadata = null, QueuePriority Priority = QueuePriority.Normal);

/// <summary>
/// Request model for releasing users from a queue.
/// </summary>
/// <param name="Count">The number of users to release.</param>
public record ReleaseUsersRequest(int Count);

/// <summary>
/// Response model for releasing users from a queue.
/// </summary>
/// <param name="ReleasedCount">The number of users that were released.</param>
public record ReleaseUsersResponse(int ReleasedCount);

#endregion
