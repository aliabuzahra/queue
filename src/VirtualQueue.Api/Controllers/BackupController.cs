using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/backup")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IBackupService backupService, ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<BackupResponse>> CreateBackup()
    {
        try
        {
            var backupResult = await _backupService.CreateBackupAsync(VirtualQueue.Application.Common.Interfaces.BackupType.Full);
            var response = new BackupResponse(backupResult.BackupId, DateTime.UtcNow, "Backup created successfully");
            
            _logger.LogInformation("Backup created: {BackupId}", backupResult.BackupId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup");
            return StatusCode(500, new { message = "Backup creation error" });
        }
    }

    [HttpPost("restore")]
    public async Task<ActionResult> RestoreBackup([FromBody] RestoreBackupRequest request)
    {
        try
        {
            await _backupService.RestoreBackupAsync(request.BackupFileName);
            _logger.LogInformation("Backup restored: {BackupFileName}", request.BackupFileName);
            return Ok(new { message = "Backup restored successfully" });
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("Backup file not found: {BackupFileName}", request.BackupFileName);
            return NotFound(new { message = "Backup file not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup {BackupFileName}", request.BackupFileName);
            return StatusCode(500, new { message = "Backup restoration error" });
        }
    }

    [HttpPost("verify")]
    public async Task<ActionResult<BackupVerificationResponse>> VerifyBackup([FromBody] VerifyBackupRequest request)
    {
        try
        {
            var isValid = await _backupService.VerifyBackupAsync(request.BackupFileName);
            var response = new BackupVerificationResponse(request.BackupFileName, isValid, DateTime.UtcNow);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying backup {BackupFileName}", request.BackupFileName);
            return StatusCode(500, new { message = "Backup verification error" });
        }
    }

    [HttpPost("cleanup")]
    public async Task<ActionResult> CleanOldBackups()
    {
        try
        {
            await _backupService.CleanOldBackupsAsync(30); // Default 30 days retention
            _logger.LogInformation("Old backups cleaned up successfully");
            return Ok(new { message = "Old backups cleaned up successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old backups");
            return StatusCode(500, new { message = "Backup cleanup error" });
        }
    }
}

public record BackupResponse(string BackupFileName, DateTime CreatedAt, string Message);
public record RestoreBackupRequest(string BackupFileName);
public record VerifyBackupRequest(string BackupFileName);
public record BackupVerificationResponse(string BackupFileName, bool IsValid, DateTime VerifiedAt);
