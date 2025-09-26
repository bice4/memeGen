using MemeGen.ClientApiService.Models;
using MemeGen.Domain.Entities;
using StackExchange.Redis;

namespace MemeGen.ClientApiService.Services;

/// <summary>
/// Service for caching images and retrieving random templates and quotes.
/// </summary>
public interface IImageCache
{
    /// <summary>
    /// Adds an image to the Redis cache with a specified expiration time.
    /// </summary>
    /// <param name="keys">Array of strings used to create a unique cache key.</param>
    /// <param name="blobName">Name of the blob to be cached.</param>
    /// <param name="expirationDate">Optional expiration time for the cache entry. If null or less than 1 minute, a default of 1 hour is used.</param>
    /// <returns></returns>
    Task AddImageToRedisCache(string[] keys, string blobName, TimeSpan? expirationDate);

    /// <summary>
    /// Gets an image from the Redis cache based on the provided keys.
    /// </summary>
    /// <param name="keys">Array of strings used to create the unique cache key.</param>
    /// <returns>The name of the cached blob if found; otherwise, null.</returns>
    Task<string?> GetImageFromRedisCache(string[] keys);

    /// <summary>
    /// Gets a random template and quote from the provided list of templates.
    /// </summary>
    /// <param name="personTemplates">List of templates associated with a person.</param>
    /// <returns>A tuple containing the selected template and quote.</returns>
    RandomTemplateAndQuote GetRandomTemplateAndQuote(List<Template> personTemplates);
}

/// <inheritdoc/>
public class ImageCache(
    ILogger<ImageCache> logger,
    IConnectionMultiplexer mux
) : IImageCache
{
    private readonly TimeSpan _defaultCacheExpirationTime = TimeSpan.FromHours(1);
    private readonly Random _random = new();
    private string? _lastQuote;

    private static string GetKeyByListOfParams(string[] keys) => string.Join("_", keys);

    /// <inheritdoc/>
    public async Task AddImageToRedisCache(string[] keys, string blobName, TimeSpan? expirationDate)
    {
        // Ensure expirationDate is at least 1 minute, otherwise use default
        if (expirationDate?.TotalMinutes < 1)
        {
            expirationDate = _defaultCacheExpirationTime;
        }

        var redis = mux.GetDatabase();
        await redis.StringSetAsync(GetKeyByListOfParams(keys), blobName, expirationDate);

        logger.LogInformation("Image added to cache with quote {Quote} and {TemplateId}.", keys[0], keys[1]);
    }

    /// <inheritdoc/>
    public async Task<string?> GetImageFromRedisCache(string[] keys)
    {
        var redis = mux.GetDatabase();

        var redisValue = await redis.StringGetAsync(GetKeyByListOfParams(keys));
        if (redisValue.IsNullOrEmpty) return null!;

        return redisValue;
    }
    
    /// <inheritdoc/>
    public RandomTemplateAndQuote GetRandomTemplateAndQuote(List<Template> personTemplates)
    {
        // Not a perfect solution but good enough for now to avoid repeating the same quote twice in a row
        var randomTemplate = personTemplates[_random.Next(personTemplates.Count)];

        string quote;
        do
        {
            quote = randomTemplate.Quotes[_random.Next(randomTemplate.Quotes.Count)];
        } while (quote == _lastQuote && randomTemplate.Quotes.Count > 1);

        _lastQuote = quote;

        return new RandomTemplateAndQuote(randomTemplate, quote);
    }
}