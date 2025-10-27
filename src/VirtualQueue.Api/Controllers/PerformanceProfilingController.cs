using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId}/performance")]
public class PerformanceProfilingController : ControllerBase
{
    private readonly IPerformanceProfilingService _profilingService;
    private readonly ILogger<PerformanceProfilingController> _logger;

    public PerformanceProfilingController(IPerformanceProfilingService profilingService, ILogger<PerformanceProfilingController> logger)
    {
        _profilingService = profilingService;
        _logger = logger;
    }

    [HttpPost("profiles")]
    public async Task<ActionResult<ProfileDto>> StartProfile(Guid tenantId, [FromBody] StartProfileRequest request)
    {
        try
        {
            var profile = await _profilingService.StartProfileAsync(request.ProfileName, Enum.Parse<VirtualQueue.Application.Common.Interfaces.ProfileType>(request.Type));
            _logger.LogInformation("Performance profile started for tenant {TenantId}: {ProfileName}", tenantId, request.ProfileName);
            return CreatedAtAction(nameof(GetProfile), new { tenantId, profileId = profile.Id }, profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting performance profile for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Profile start error" });
        }
    }

    [HttpPost("profiles/{profileId}/stop")]
    public async Task<ActionResult<ProfileDto>> StopProfile(Guid tenantId, Guid profileId)
    {
        try
        {
            var profile = await _profilingService.StopProfileAsync(profileId);
            _logger.LogInformation("Performance profile stopped for tenant {TenantId}: {ProfileId}", tenantId, profileId);
            return Ok(profile);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "Profile not found or already stopped" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping performance profile {ProfileId} for tenant {TenantId}", profileId, tenantId);
            return StatusCode(500, new { message = "Profile stop error" });
        }
    }

    [HttpGet("profiles/{profileId}")]
    public async Task<ActionResult<ProfileDto>> GetProfile(Guid tenantId, Guid profileId)
    {
        try
        {
            var profile = await _profilingService.GetProfileAsync(profileId);
            if (profile == null)
                return NotFound(new { message = "Profile not found" });
            
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance profile {ProfileId} for tenant {TenantId}", profileId, tenantId);
            return StatusCode(500, new { message = "Profile retrieval error" });
        }
    }

    [HttpGet("profiles")]
    public async Task<ActionResult<List<ProfileDto>>> GetProfiles(
        Guid tenantId,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            ProfileType? profileType = null;
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<ProfileType>(type, true, out var parsedType))
            {
                profileType = parsedType;
            }
            
            var profiles = await _profilingService.GetProfilesAsync(tenantId, profileType, startDate, endDate);
            return Ok(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance profiles for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Profiles retrieval error" });
        }
    }

    [HttpGet("report")]
    public async Task<ActionResult<PerformanceReport>> GeneratePerformanceReport(
        Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var report = await _profilingService.GeneratePerformanceReportAsync(tenantId, startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance report for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Performance report generation error" });
        }
    }

    [HttpGet("bottlenecks")]
    public async Task<ActionResult<List<PerformanceBottleneck>>> IdentifyBottlenecks(Guid tenantId)
    {
        try
        {
            var bottlenecks = await _profilingService.IdentifyBottlenecksAsync(tenantId);
            return Ok(bottlenecks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying bottlenecks for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Bottleneck identification error" });
        }
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<PerformanceRecommendation>> GetPerformanceRecommendations(Guid tenantId)
    {
        try
        {
            var recommendation = await _profilingService.GetPerformanceRecommendationsAsync(tenantId);
            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance recommendations for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Recommendations retrieval error" });
        }
    }

    [HttpPost("continuous/start")]
    public async Task<ActionResult> StartContinuousProfiling(Guid tenantId, [FromBody] StartContinuousProfilingRequest request)
    {
        try
        {
            var success = await _profilingService.StartContinuousProfilingAsync(tenantId, request.Config);
            
            if (success)
            {
                _logger.LogInformation("Continuous profiling started for tenant {TenantId}", tenantId);
                return Ok(new { message = "Continuous profiling started successfully" });
            }
            else
            {
                return BadRequest(new { message = "Continuous profiling start failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting continuous profiling for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Continuous profiling start error" });
        }
    }

    [HttpPost("continuous/stop")]
    public async Task<ActionResult> StopContinuousProfiling(Guid tenantId)
    {
        try
        {
            var success = await _profilingService.StopContinuousProfilingAsync(tenantId);
            
            if (success)
            {
                _logger.LogInformation("Continuous profiling stopped for tenant {TenantId}", tenantId);
                return Ok(new { message = "Continuous profiling stopped successfully" });
            }
            else
            {
                return BadRequest(new { message = "Continuous profiling stop failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping continuous profiling for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Continuous profiling stop error" });
        }
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<ProfilingMetrics>> GetPerformanceMetrics(
        Guid tenantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var metrics = await _profilingService.GetPerformanceMetricsAsync(tenantId, startDate, endDate);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics for tenant {TenantId}", tenantId);
            return StatusCode(500, new { message = "Metrics retrieval error" });
        }
    }
}

public record StartProfileRequest(string ProfileName, string Type);
public record StartContinuousProfilingRequest(ContinuousProfilingConfig Config);
