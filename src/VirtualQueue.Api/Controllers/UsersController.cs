using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Commands.Users;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.Users;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(Guid tenantId, [FromBody] CreateUserRequest request)
    {
        try
        {
            var command = new CreateUserCommand(
                tenantId,
                request.Username,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Role.ToString());

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUser), new { tenantId, userId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User creation failed: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid user data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "User creation error" });
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid tenantId, Guid userId)
    {
        try
        {
            var query = new GetUserByIdQuery(userId);
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound(new { message = "User not found" });
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} for tenant {TenantId}", userId, tenantId);
            return StatusCode(500, new { message = "User retrieval error" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers(Guid tenantId)
    {
        try
        {
            var query = new GetUsersByTenantQuery(tenantId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Users retrieval error" });
        }
    }

    [HttpPut("{userId}/profile")]
    public async Task<ActionResult<UserDto>> UpdateUserProfile(Guid tenantId, Guid userId, [FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var command = new UpdateUserProfileCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.ProfileImageUrl);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User profile update failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid profile data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile {UserId} for tenant {TenantId}", userId, tenantId);
            return StatusCode(500, new { message = "Profile update error" });
        }
    }
}
