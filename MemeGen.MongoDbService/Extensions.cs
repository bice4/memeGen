using MemeGen.MongoDbService.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MemeGen.MongoDbService;

public static class Extensions
{
    public static void AddMongoDbServices(this IHostApplicationBuilder builder,
        string connectionName = "memeGenTemplates")
    {
        builder.AddMongoDBClient(connectionName: connectionName);
        
        builder.Services.AddSingleton<ITemplateRepository, TemplateRepository>();
        builder.Services.AddSingleton<IImageGenerationRepository, ImageGenerationRepository>();
    }
}