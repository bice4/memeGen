using MemeGen.Common.Constants;
using MemeGen.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MemeGen.ImageProcessor.Persistent.MongoDb;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken);
}

public class TemplateRepository(IMongoClient client) : ITemplateRepository
{
    public Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.Find(template => template.Id == id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)!;
    }

    private IMongoCollection<Template> GetCollection()
    {
        var database = client.GetDatabase(MongoDbConstants.DatabaseName);
        return database.GetCollection<Template>(MongoDbConstants.TemplateCollectionName);
    }
}