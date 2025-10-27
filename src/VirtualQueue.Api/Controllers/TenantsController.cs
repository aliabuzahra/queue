using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Commands.Tenants;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.Tenants;

namespace VirtualQueue.Api.Controllers;

/// <summary>
/// Controller for managing tenant operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TenantsController : ControllerBase
{
    #region Fields
    private readonly IMediator _mediator;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for handling commands and queries.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the mediator is null.
    /// </exception>
    public TenantsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="request">The tenant creation request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The created tenant information.
    /// </returns>
    /// <response code="201">Tenant created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TenantDto>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            var command = new CreateTenantCommand(request.Name, request.Domain);
            var result = await _mediator.Send(command, cancellationToken);
            
            return CreatedAtAction(nameof(GetTenant), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the tenant");
        }
    }

    /// <summary>
    /// Retrieves a tenant by their unique identifier.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The tenant information if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <response code="200">Returns the tenant information.</response>
    /// <response code="404">Tenant not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TenantDto>> GetTenant(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid tenant ID");

            var query = new GetTenantByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result == null)
                return NotFound($"Tenant with ID {id} not found");
                
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the tenant");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAllTenants()
    {
        var query = new GetAllTenantsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        var command = new UpdateTenantCommand(id, request.Name, request.Domain);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTenant(Guid id)
    {
        var command = new DeleteTenantCommand(id);
        var deleted = await _mediator.Send(command);
        
        if (!deleted)
            return NotFound();
            
        return NoContent();
    }
}

/// <summary>
/// Request model for creating a new tenant.
/// </summary>
/// <param name="Name">The name of the tenant.</param>
/// <param name="Domain">The domain of the tenant.</param>
public record CreateTenantRequest(string Name, string Domain);

/// <summary>
/// Request model for updating an existing tenant.
/// </summary>
/// <param name="Name">The name of the tenant.</param>
/// <param name="Domain">The domain of the tenant.</param>
public record UpdateTenantRequest(string Name, string Domain);

#endregion
