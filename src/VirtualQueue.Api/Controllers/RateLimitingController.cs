using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/rate-limiting")]
public class RateLimitingController : ControllerBase
{
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<RateLimitingController> _logger;

    public RateLimitingController(IRateLimitingService rateLimitingService, ILogger<RateLimitingController> logger)
    {
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    [HttpPost("check")]
    public async Task<ActionResult<RateLimitCheckResponse>> CheckRateLimit([FromBody] RateLimitCheckRequest request)
    {
        try
        {
            var isAllowed = await _rateLimitingService.IsAllowedAsync(request.Key, request.Limit, request.Window);
            var remaining = await _rateLimitingService.GetRemainingRequestsAsync(request.Key, request.Limit, request.Window);
            var rateLimitInfo = await _rateLimitingService.GetRateLimitInfoAsync(request.Key, request.Limit, request.Window);
            
            var response = new RateLimitCheckResponse(
                isAllowed, 
                remaining, 
                rateLimitInfo.RemainingRequests,
                rateLimitInfo.ResetTime,
                DateTime.UtcNow);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key {Key}", request.Key);
            return StatusCode(500, new { message = "Rate limit check error" });
        }
    }

    [HttpGet("info/{key}")]
    public async Task<ActionResult<RateLimitInfoResponse>> GetRateLimitInfo(string key, [FromQuery] int limit = 100, [FromQuery] int windowMinutes = 1)
    {
        try
        {
            var window = TimeSpan.FromMinutes(windowMinutes);
            var rateLimitInfo = await _rateLimitingService.GetRateLimitInfoAsync(key, limit, window);
            
            var response = new RateLimitInfoResponse(
                rateLimitInfo.IsAllowed,
                rateLimitInfo.RemainingRequests,
                rateLimitInfo.TotalRequests,
                rateLimitInfo.ResetTime,
                rateLimitInfo.Window,
                DateTime.UtcNow);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rate limit info for key {Key}", key);
            return StatusCode(500, new { message = "Rate limit info retrieval error" });
        }
    }

    [HttpPost("reset")]
    public async Task<ActionResult> ResetRateLimit([FromBody] ResetRateLimitRequest request)
    {
        try
        {
            await _rateLimitingService.ResetRateLimitAsync(request.Key);
            _logger.LogInformation("Rate limit reset for key {Key}", request.Key);
            return Ok(new { message = "Rate limit reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit for key {Key}", request.Key);
            return StatusCode(500, new { message = "Rate limit reset error" });
        }
    }

    [HttpPost("configure")]
    public async Task<ActionResult> ConfigureRateLimit([FromBody] ConfigureRateLimitRequest request)
    {
        try
        {
            await _rateLimitingService.SetRateLimitAsync(request.Key, request.Limit, request.Window);
            _logger.LogInformation("Rate limit configured for key {Key}: {Limit} requests per {Window}", 
                request.Key, request.Limit, request.Window);
            return Ok(new { message = "Rate limit configured successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring rate limit for key {Key}", request.Key);
            return StatusCode(500, new { message = "Rate limit configuration error" });
        }
    }
}

public record RateLimitCheckRequest(string Key, int Limit, TimeSpan Window);
public record RateLimitCheckResponse(bool IsAllowed, int Remaining, int TotalRequests, DateTime ResetTime, DateTime CheckedAt);
public record RateLimitInfoResponse(bool IsAllowed, int Remaining, int Total, DateTime ResetTime, TimeSpan Window, DateTime RetrievedAt);
public record ResetRateLimitRequest(string Key);
public record ConfigureRateLimitRequest(string Key, int Limit, TimeSpan Window);
