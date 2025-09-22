using MemeGen.Common.Constants;
using MemeGen.Domain.Entities;
using MongoDB.Driver;

namespace MemeGen.ClientApiService.Persistent.MongoDb;

public interface IImageGenerationRepository
{
    Task CreateAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken);

    Task<ImageGeneration?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);
}

public class ImageGenerationRepository(IMongoClient client) : IImageGenerationRepository
{
    public Task CreateAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.InsertOneAsync(imageGeneration, cancellationToken: cancellationToken);
    }

    public Task<ImageGeneration?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
    {
        var filter = Builders<ImageGeneration>.Filter.Eq(x => x.CorrelationId, correlationId);
        return GetCollection().Find(filter).FirstOrDefaultAsync(cancellationToken)!;
    }

    private IMongoCollection<ImageGeneration> GetCollection()
    {
        var database = client.GetDatabase(MongoDbConstants.DatabaseName);
        return database.GetCollection<ImageGeneration>(MongoDbConstants.ImageGenerationCollectionName);
    }
}