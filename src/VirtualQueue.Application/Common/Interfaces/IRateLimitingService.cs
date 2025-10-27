namespace VirtualQueue.Application.Common.Interfaces;

public interface IRateLimitingService
{
    Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
    Task<int> GetRemainingRequestsAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
    Task ResetRateLimitAsync(string key, CancellationToken cancellationToken = default);
    Task SetRateLimitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
}

public record RateLimitInfo(
    bool IsAllowed,
    int RemainingRequests,
    int TotalRequests,
    DateTime ResetTime,
    TimeSpan Window
);
