using MediatR;
using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.Dashboard;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(IMediator mediator, ILogger<AdminDashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard(
        Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] List<string>? metrics = null)
    {
        try
        {
            var query = new GetAdminDashboardQuery(tenantId, startDate, endDate, metrics);
            var result = await _mediator.Send(query);
            
            _logger.LogInformation("Admin dashboard generated for tenant {TenantId}", tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating admin dashboard for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Dashboard generation error" });
        }
    }

    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverview>> GetDashboardOverview(Guid tenantId)
    {
        try
        {
            var query = new GetAdminDashboardQuery(tenantId);
            var dashboard = await _mediator.Send(query);
            
            _logger.LogInformation("Dashboard overview retrieved for tenant {TenantId}", tenantId);
            return Ok(dashboard.Overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard overview for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Dashboard overview retrieval error" });
        }
    }

    [HttpGet("widgets")]
    public async Task<ActionResult<List<DashboardWidget>>> GetDashboardWidgets(Guid tenantId)
    {
        try
        {
            var query = new GetAdminDashboardQuery(tenantId);
            var dashboard = await _mediator.Send(query);
            
            _logger.LogInformation("Dashboard widgets retrieved for tenant {TenantId}", tenantId);
            return Ok(dashboard.Widgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard widgets for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Dashboard widgets retrieval error" });
        }
    }

    [HttpGet("charts")]
    public async Task<ActionResult<List<DashboardChart>>> GetDashboardCharts(
        Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = new GetAdminDashboardQuery(tenantId, startDate, endDate);
            var dashboard = await _mediator.Send(query);
            
            _logger.LogInformation("Dashboard charts retrieved for tenant {TenantId}", tenantId);
            return Ok(dashboard.Charts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard charts for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Dashboard charts retrieval error" });
        }
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<List<DashboardAlert>>> GetDashboardAlerts(Guid tenantId)
    {
        try
        {
            var query = new GetAdminDashboardQuery(tenantId);
            var dashboard = await _mediator.Send(query);
            
            _logger.LogInformation("Dashboard alerts retrieved for tenant {TenantId}", tenantId);
            return Ok(dashboard.Alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard alerts for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Dashboard alerts retrieval error" });
        }
    }

    [HttpPost("export")]
    public Task<ActionResult> ExportDashboard(
        Guid tenantId, 
        [FromBody] DashboardExportRequest request)
    {
        try
        {
            _logger.LogInformation("Dashboard export requested for tenant {TenantId} in format {Format}", 
                tenantId, request.Format);
            
            // Mock implementation
            var exportData = new
            {
                TenantId = tenantId,
                Format = request.Format,
                GeneratedAt = DateTime.UtcNow,
                Data = "Mock export data"
            };
            
            return Task.FromResult<ActionResult>(Ok(exportData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting dashboard for tenant {TenantId}", tenantId);
            return Task.FromResult<ActionResult>(StatusCode(500, new { message = "Dashboard export error" }));
        }
    }
}

