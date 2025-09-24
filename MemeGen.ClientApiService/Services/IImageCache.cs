using MemeGen.ClientApiService.Models;
using MemeGen.Domain.Entities;
using StackExchange.Redis;

namespace MemeGen.ClientApiService.Services;

public interface IImageCache
{
    Task AddImageToCacheAsync(string[] keys, string blobName, TimeSpan? expirationDate,
        CancellationToken cancellationToken);

    Task<string?> GetImageFromCacheAsync(string[] keys, CancellationToken cancellationToken);

    RandomTemplateAndQuote GetRandomTemplateAndQuote(List<Template> personTemplates);
}

public class ImageCache(
    ILogger<ImageCache> logger,
    IConnectionMultiplexer mux
) : IImageCache
{
    private readonly TimeSpan _defaultCacheExpirationTime = TimeSpan.FromHours(1);
    private readonly Random _random = new();
    private string? _lastQuote;

    private static string GetKeyByListOfParams(string[] keys) => string.Join("_", keys);

    public async Task AddImageToCacheAsync(string[] keys, string blobName, TimeSpan? expirationDate,
        CancellationToken cancellationToken)
    {
        if (expirationDate?.TotalMinutes < 1)
        {
            expirationDate = _defaultCacheExpirationTime;
        }

        var redis = mux.GetDatabase();
        await redis.StringSetAsync(GetKeyByListOfParams(keys), blobName, expirationDate);

        logger.LogInformation("Image added to cache with quote {Quote} and {TemplateId}.", keys[0], keys[1]);
    }

    public async Task<string?> GetImageFromCacheAsync(string[] keys,
        CancellationToken cancellationToken)
    {
        var redis = mux.GetDatabase();

        var redisValue = await redis.StringGetAsync(GetKeyByListOfParams(keys));
        if (redisValue.IsNullOrEmpty) return null!;

        return redisValue;
    }

    public RandomTemplateAndQuote GetRandomTemplateAndQuote(List<Template> personTemplates)
    {
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