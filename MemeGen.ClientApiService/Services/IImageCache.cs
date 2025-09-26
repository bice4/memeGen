using MemeGen.ClientApiService.Models;
using MemeGen.Domain.Entities;
using MemeGen.RedisService;

namespace MemeGen.ClientApiService.Services;

/// <summary>
/// Service for caching images and retrieving random templates and quotes.
/// </summary>
public interface IImageCache
{
    /// <summary>
    /// Adds an image to the Redis cache with a specified expiration time.
    /// </summary>
    /// <param name="imageGeneration"><see cref="ImageGeneration"/> object containing image details to be cached.</param>
    /// <param name="expirationDate">Optional expiration time for the cache entry. If null or less than 1 minute, a default of 1 hour is used.</param>
    /// <returns></returns>
    Task AddImageToRedisCache(ImageGeneration imageGeneration, TimeSpan? expirationDate);

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
    IRedisRepository redisRepository
) : IImageCache
{
    private readonly TimeSpan _defaultCacheExpirationTime = TimeSpan.FromHours(1);
    private readonly Random _random = new();
    private string? _lastQuote;
    
    /// <inheritdoc/>
    public async Task AddImageToRedisCache(ImageGeneration imageGeneration, TimeSpan? expirationDate)
    {
        // Ensure expirationDate is at least 1 minute, otherwise use default
        if (expirationDate?.TotalMinutes < 1)
        {
            expirationDate = _defaultCacheExpirationTime;
        }

        await redisRepository.SetValueAsync(imageGeneration.KeyFromImageGeneration(), imageGeneration.BlobFileName!,
            expirationDate);
    }

    /// <inheritdoc/>
    public Task<string?> GetImageFromRedisCache(string[] keys) => redisRepository.GetValueAsync(keys);

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