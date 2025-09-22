using MemeGen.Common.Constants;
using MemeGen.Domain.Entities;
using MongoDB.Driver;

namespace MemeGen.ImageProcessor.Persistent.MongoDb;

public interface IImageGenerationRepository
{
    Task<ImageGeneration?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);

    Task UpdateAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken);
}

public class ImageGenerationRepository(IMongoClient client) : IImageGenerationRepository
{
    public Task<ImageGeneration?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
    {
        var filter = Builders<ImageGeneration>.Filter.Eq(x => x.CorrelationId, correlationId);
        return GetCollection().Find(filter).FirstOrDefaultAsync(cancellationToken)!;
    }

    public Task UpdateAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken)
    {
        var filter = Builders<ImageGeneration>.Filter.Eq(x => x.CorrelationId, imageGeneration.CorrelationId);
        return GetCollection().ReplaceOneAsync(filter, imageGeneration, cancellationToken: cancellationToken);
    }

    private IMongoCollection<ImageGeneration> GetCollection()
    {
        var database = client.GetDatabase(MongoDbConstants.DatabaseName);
        return database.GetCollection<ImageGeneration>(MongoDbConstants.ImageGenerationCollectionName);
    }
}