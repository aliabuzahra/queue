using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

public class RedisRateLimitingService : IRateLimitingService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<RedisRateLimitingService> _logger;

    public RedisRateLimitingService(ICacheService cacheService, ILogger<RedisRateLimitingService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        try
        {
            var rateLimitKey = $"rate_limit:{key}";
            var currentTime = DateTime.UtcNow;
            var windowStart = currentTime.Subtract(window);

            // Get current count
            var currentCountStr = await _cacheService.GetAsync<string>($"count:{rateLimitKey}", cancellationToken);
            var windowStartTimeStr = await _cacheService.GetAsync<string>($"window_start:{rateLimitKey}", cancellationToken);
            
            var currentCount = int.TryParse(currentCountStr, out var count) ? count : 0;
            var windowStartTime = DateTime.TryParse(windowStartTimeStr, out var startTime) ? startTime : (DateTime?)null;

            // If window has expired or doesn't exist, reset
            if (!windowStartTime.HasValue || windowStartTime.Value < windowStart)
            {
                await _cacheService.SetAsync($"count:{rateLimitKey}", "1", window, cancellationToken);
                await _cacheService.SetAsync($"window_start:{rateLimitKey}", currentTime.ToString("O"), window, cancellationToken);
                return true;
            }

            // If within window, check if limit exceeded
            if (currentCount >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for key {Key}. Current count: {Count}, Limit: {Limit}", key, currentCount, limit);
                return false;
            }

            // Increment counter
            await _cacheService.SetAsync($"count:{rateLimitKey}", (currentCount + 1).ToString(), window, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key {Key}", key);
            // Fail open - allow request if rate limiting fails
            return true;
        }
    }

    public async Task<int> GetRemainingRequestsAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        try
        {
            var rateLimitKey = $"rate_limit:{key}";
            var currentCountStr = await _cacheService.GetAsync<string>($"count:{rateLimitKey}", cancellationToken);
            var currentCount = int.TryParse(currentCountStr, out var count) ? count : 0;
            return Math.Max(0, limit - currentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining requests for key {Key}", key);
            return limit; // Return full limit if error
        }
    }

    public async Task<RateLimitInfo> GetRateLimitInfoAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        try
        {
            var rateLimitKey = $"rate_limit:{key}";
            var currentCountStr = await _cacheService.GetAsync<string>($"count:{rateLimitKey}", cancellationToken);
            var windowStartTimeStr = await _cacheService.GetAsync<string>($"window_start:{rateLimitKey}", cancellationToken);
            
            var currentCount = int.TryParse(currentCountStr, out var count) ? count : 0;
            var windowStartTime = DateTime.TryParse(windowStartTimeStr, out var startTime) ? startTime : DateTime.UtcNow;
            
            var resetTime = windowStartTime.Add(window);
            var isAllowed = currentCount < limit;
            var remaining = Math.Max(0, limit - currentCount);

            return new RateLimitInfo(
                isAllowed,
                remaining,
                currentCount,
                resetTime,
                window
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rate limit info for key {Key}", key);
            return new RateLimitInfo(true, limit, 0, DateTime.UtcNow.Add(window), window);
        }
    }

    public async Task ResetRateLimitAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var rateLimitKey = $"rate_limit:{key}";
            await _cacheService.RemoveAsync($"count:{rateLimitKey}", cancellationToken);
            await _cacheService.RemoveAsync($"window_start:{rateLimitKey}", cancellationToken);
            
            _logger.LogInformation("Rate limit reset for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit for key {Key}", key);
        }
    }

    public async Task SetRateLimitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        try
        {
            var rateLimitKey = $"rate_limit:{key}";
            await _cacheService.SetAsync($"count:{rateLimitKey}", "0", window, cancellationToken);
            await _cacheService.SetAsync($"window_start:{rateLimitKey}", DateTime.UtcNow.ToString("O"), window, cancellationToken);
            
            _logger.LogInformation("Rate limit set for key {Key}: {Limit} requests per {Window}", key, limit, window);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting rate limit for key {Key}", key);
        }
    }
}
