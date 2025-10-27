namespace VirtualQueue.Application.Common.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task SetUserPositionAsync(Guid queueId, string userIdentifier, int position, CancellationToken cancellationToken = default);
    Task<int?> GetUserPositionAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default);
    Task RemoveUserPositionAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default);
}
