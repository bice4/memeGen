using MemeGen.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MemeGen.RedisService;

public static class Extensions
{
    public static void AddRedisServices(this IHostApplicationBuilder builder,
        string connectionName = "imageRedisCache")
    {
        builder.AddRedisClient(connectionName: connectionName);
        builder.Services.AddSingleton<IRedisRepository, RedisRepository>();
    }
    
    public static string KeyFromImageGeneration(this ImageGeneration generation)
        => $"{generation.TemplateId}_{generation.Quote}_{generation.ConfigurationThumbprint}";
}