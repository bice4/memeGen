using MemeGen.ClientApiService.Models;
using MemeGen.Domain.Entities;
using StackExchange.Redis;

namespace MemeGen.ClientApiService.Services;

public interface IImageCache
{
    Task AddImageToCacheAsync(string templateId, string quote, string blobName, CancellationToken cancellationToken);

    Task<string?> GetImageFromCacheAsync(string templateId, string quote, CancellationToken cancellationToken);

    RandomTemplateAndQuote GetRandomTemplateAndQuote(List<Template> personTemplates);
}

public class ImageCache(
    ILogger<ImageCache> logger,
    IConnectionMultiplexer mux
) : IImageCache
{
    private readonly TimeSpan _cacheExpirationTime = TimeSpan.FromHours(1);
    private readonly Random _random = new();
    private string? _lastQuote;


    public async Task AddImageToCacheAsync(string templateId, string quote, string blobName,
        CancellationToken cancellationToken)
    {
        var redis = mux.GetDatabase();
        await redis.StringSetAsync($"{templateId}_{quote}", blobName, _cacheExpirationTime);

        logger.LogInformation("Image added to cache with quote {Quote} and {TemplateId}.", quote, templateId);
    }

    public async Task<string?> GetImageFromCacheAsync(string templateId, string quote,
        CancellationToken cancellationToken)
    {
        var redis = mux.GetDatabase();

        var redisValue = await redis.StringGetAsync($"{templateId}_{quote}");

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