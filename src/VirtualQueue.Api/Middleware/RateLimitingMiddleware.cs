using Microsoft.AspNetCore.Http;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, IRateLimitingService rateLimitingService, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Get rate limiting key (IP address or tenant ID)
            var rateLimitKey = GetRateLimitKey(context);
            
            // Define rate limits based on endpoint
            var (limit, window) = GetRateLimitForEndpoint(context.Request.Path);
            
            // Check if request is allowed
            var isAllowed = await _rateLimitingService.IsAllowedAsync(rateLimitKey, limit, window);
            
            if (!isAllowed)
            {
                var rateLimitInfo = await _rateLimitingService.GetRateLimitInfoAsync(rateLimitKey, limit, window);
                
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.Headers["Retry-After"] = ((int)rateLimitInfo.ResetTime.Subtract(DateTime.UtcNow).TotalSeconds).ToString();
                context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = rateLimitInfo.RemainingRequests.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = rateLimitInfo.ResetTime.ToString("R");
                
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }
            
            // Add rate limit headers to response
            var remainingRequests = await _rateLimitingService.GetRemainingRequestsAsync(rateLimitKey, limit, window);
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remainingRequests.ToString();
            
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limiting middleware");
            // Fail open - allow request if rate limiting fails
            await _next(context);
        }
    }

    private string GetRateLimitKey(HttpContext context)
    {
        // Try to get tenant ID from context first
        var tenantContext = context.RequestServices.GetService<ITenantContext>();
        if (tenantContext?.TenantId != null)
        {
            return $"tenant:{tenantContext.TenantId}";
        }
        
        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private (int limit, TimeSpan window) GetRateLimitForEndpoint(PathString path)
    {
        var pathString = path.Value?.ToLowerInvariant() ?? "";
        
        // Define different rate limits for different endpoints
        return pathString switch
        {
            var p when p.Contains("/enqueue") => (100, TimeSpan.FromMinutes(1)), // 100 requests per minute for enqueue
            var p when p.Contains("/release") => (50, TimeSpan.FromMinutes(1)),  // 50 requests per minute for release
            var p when p.Contains("/analytics") => (20, TimeSpan.FromMinutes(1)), // 20 requests per minute for analytics
            var p when p.Contains("/tenants") => (10, TimeSpan.FromMinutes(1)),   // 10 requests per minute for tenant operations
            _ => (200, TimeSpan.FromMinutes(1)) // Default: 200 requests per minute
        };
    }
}
