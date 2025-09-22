using MemeGen.Common.Constants;
using MemeGen.Domain.Entities;
using MongoDB.Driver;

namespace MemeGen.ApiService.Persistent.MongoDb;

public interface IImageGenerationRepository
{
    Task<List<string?>> GetAllAsync(CancellationToken cancellationToken);
}

public class ImageGenerationRepository(IMongoClient client) : IImageGenerationRepository
{
    private IMongoCollection<ImageGeneration> GetCollection()
    {
        var database = client.GetDatabase(MongoDbConstants.DatabaseName);
        return database.GetCollection<ImageGeneration>(MongoDbConstants.ImageGenerationCollectionName);
    }

    public async Task<List<string?>> GetAllAsync(CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        var all = await collection.Find(x => x.Status == ImageGenerationStatus.Completed)
            .ToListAsync(cancellationToken: cancellationToken);
        return all.Select(x => x.BlobFileName).ToList();
    }
}