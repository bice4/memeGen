using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MemeGen.RedisService;

/// <summary>
/// Repository for interacting with Redis cache.
/// </summary>
public interface IRedisRepository
{
    /// <summary>
    /// Gets a string value from Redis by key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The string value if found; otherwise, null.</returns>
    Task<string?> GetValueAsync(string key);
    
    /// <summary>
    /// Gets a string value from Redis by a complex key made of multiple parts.
    /// </summary>
    /// <param name="complexKey">Array of strings to form the key.</param>
    /// <returns>The string value if found; otherwise, null.</returns>
    Task<string?> GetValueAsync(string[] complexKey);

    /// <summary>
    /// Sets a string value in Redis with an optional expiration time.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The string value to set.</param>
    /// <param name="expiry">Optional expiration time for the key. If null, the key will not expire.</param>
    /// <returns></returns>
    Task SetValueAsync(string key, string value, TimeSpan? expiry = null);
}

/// <inheritdoc/>
public class RedisRepository(ILogger<RedisRepository> logger, IConnectionMultiplexer connectionMultiplexer)
    : IRedisRepository
{
    /// <inheritdoc/>
    public async Task<string?> GetValueAsync(string key)
    {
        var redis = connectionMultiplexer.GetDatabase();
        var value = await redis.StringGetAsync(key);
        
        if (!value.IsNullOrEmpty) return value;
        
        logger.LogInformation("Value not found in Redis for key: {Key}", key);
        return null;
    }

    /// <inheritdoc/>
    public async Task<string?> GetValueAsync(string[] complexKey)
    {
        var key = string.Join('_', complexKey);
        var redis = connectionMultiplexer.GetDatabase();
        var value = await redis.StringGetAsync(key);
        
        if (!value.IsNullOrEmpty) return value;
        
        logger.LogInformation("Value not found in Redis for key: {Key}", key);
        return null;
    }

    /// <inheritdoc/>
    public async Task SetValueAsync(string key, string value, TimeSpan? expiry = null)
    {
        var redis = connectionMultiplexer.GetDatabase();
        await redis.StringSetAsync(key, value, expiry);

        logger.LogInformation("Value set in Redis with key: {Key}", key);
    }
}