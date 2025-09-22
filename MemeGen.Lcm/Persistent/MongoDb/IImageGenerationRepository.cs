using MemeGen.Common.Constants;
using MemeGen.Domain.Entities;
using MongoDB.Driver;

namespace MemeGen.Lcm.Persistent.MongoDb;

public interface IImageGenerationRepository
{
    Task<List<ImageGeneration>> GetAllAsync(CancellationToken cancellationToken);

    Task DeleteAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken);
}

public class ImageGenerationRepository(IMongoClient client) : IImageGenerationRepository
{
    public Task<List<ImageGeneration>> GetAllAsync(CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.Find(template => true).ToListAsync(cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.DeleteOneAsync(x => x.Id == imageGeneration.Id, cancellationToken);
    }

    private IMongoCollection<ImageGeneration> GetCollection()
    {
        var database = client.GetDatabase(MongoDbConstants.DatabaseName);
        return database.GetCollection<ImageGeneration>(MongoDbConstants.ImageGenerationCollectionName);
    }
}