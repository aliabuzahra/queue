using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Infrastructure.Services;

/// <summary>
/// Service for Redis-based caching operations.
/// </summary>
public class RedisCacheService : ICacheService
{
    #region Fields
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the cache is null.
    /// </exception>
    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Retrieves a value from the cache by key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The cached value if found; otherwise, null.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the key is null or empty.
    /// </exception>
    /// <exception cref="JsonException">
    /// Thrown when the cached value cannot be deserialized.
    /// </exception>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        ValidateKey(key);
        
        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(value))
                return null;

            return JsonSerializer.Deserialize<T>(value, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize cached value for key '{key}'", ex);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var options = new DistributedCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Default 1 hour
        }

        await _cache.SetStringAsync(key, json, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync(key, cancellationToken);
        return !string.IsNullOrEmpty(value);
    }

    public async Task SetUserPositionAsync(Guid queueId, string userIdentifier, int position, CancellationToken cancellationToken = default)
    {
        var key = GetUserPositionKey(queueId, userIdentifier);
        await SetAsync(key, position.ToString(), TimeSpan.FromMinutes(30), cancellationToken);
    }

    public async Task<int?> GetUserPositionAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default)
    {
        var key = GetUserPositionKey(queueId, userIdentifier);
        var value = await GetAsync<string>(key, cancellationToken);
        return value != null ? int.Parse(value) : null;
    }

    public async Task RemoveUserPositionAsync(Guid queueId, string userIdentifier, CancellationToken cancellationToken = default)
    {
        var key = GetUserPositionKey(queueId, userIdentifier);
        await RemoveAsync(key, cancellationToken);
    }

    #endregion

    #region Private Methods
    /// <summary>
    /// Validates a cache key.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the key is null or empty.
    /// </exception>
    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
    }

    /// <summary>
    /// Generates a cache key for user position.
    /// </summary>
    /// <param name="queueId">The queue identifier.</param>
    /// <param name="userIdentifier">The user identifier.</param>
    /// <returns>
    /// The generated cache key.
    /// </returns>
    private static string GetUserPositionKey(Guid queueId, string userIdentifier)
    {
        return $"queue:{queueId}:user:{userIdentifier}:position";
    }
    #endregion
}
