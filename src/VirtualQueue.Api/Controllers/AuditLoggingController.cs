using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
public class AuditLoggingController : ControllerBase
{
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly ILogger<AuditLoggingController> _logger;

    public AuditLoggingController(IAuditLoggingService auditLoggingService, ILogger<AuditLoggingController> logger)
    {
        _auditLoggingService = auditLoggingService;
        _logger = logger;
    }

    [HttpPost("log")]
    public async Task<ActionResult> LogAction([FromBody] LogActionRequest request)
    {
        try
        {
            await _auditLoggingService.LogActionAsync(
                request.Action,
                request.EntityType,
                request.EntityId?.ToString() ?? "",
                request.UserId,
                null, // No UserIpAddress available
                request.TenantId);
            
            _logger.LogInformation("Audit log created for tenant {TenantId}, user {UserId}, action {Action}", 
                request.TenantId, request.UserId, request.Action);
            return Ok(new { message = "Audit log created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for tenant {TenantId}", request.TenantId);
            return StatusCode(500, new { message = "Audit log creation error" });
        }
    }

    [HttpGet("logs/{tenantId}")]
    public async Task<ActionResult<AuditLogsResponse>> GetAuditLogs(
        Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null)
    {
        try
        {
            var logs = await _auditLoggingService.GetAuditLogsAsync(tenantId, startDate, endDate, entityType);
            var response = new AuditLogsResponse(logs, DateTime.UtcNow);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Audit log retrieval error" });
        }
    }

    [HttpGet("logs/entry/{logId}")]
    public async Task<ActionResult<AuditLogEntry>> GetAuditLogEntry(Guid logId)
    {
        try
        {
            var logEntry = await _auditLoggingService.GetAuditLogEntryAsync(logId);
            if (logEntry == null)
                return NotFound(new { message = "Audit log entry not found" });
            
            return Ok(logEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log entry {LogId}", logId);
            return StatusCode(500, new { message = "Audit log entry retrieval error" });
        }
    }
}

public record LogActionRequest(
    Guid TenantId,
    string UserId,
    string Action,
    string EntityType,
    Guid? EntityId,
    string Details);

public record AuditLogsResponse(List<AuditLogEntry> Logs, DateTime RetrievedAt);
