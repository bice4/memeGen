using MemeGen.Domain.Entities;
using MongoDB.Driver;

namespace MemeGen.MongoDbService.Repositories;

public interface IImageGenerationRepository
{
    Task CreateAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken);

    Task<ImageGeneration?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);
    
    Task<List<ImageGeneration>> GetAllAsync(CancellationToken cancellationToken);

    Task DeleteAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken);
    
    Task<List<string?>> GetBlobNamesByPersonIdAsync(int personId, CancellationToken cancellationToken);
    
    Task UpdateAsync(ImageGeneration imageGeneration, CancellationToken cancellationToken);
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
    
    public async Task<List<string?>> GetBlobNamesByPersonIdAsync(int personId, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        var all = await collection.Find(x => x.Status == ImageGenerationStatus.Completed && x.PersonId == personId)
            .ToListAsync(cancellationToken: cancellationToken);
        return all.Select(x => x.BlobFileName).ToList();
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