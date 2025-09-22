using MemeGen.Common.Constants;
using MemeGen.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MemeGen.ApiService.Persistent.MongoDb;

public interface ITemplateRepository
{
    Task CreateAsync(Template template, CancellationToken cancellationToken);

    Task<List<Template>> GetAllAsync(CancellationToken cancellationToken);

    Task<Template> GetByIdAsync(ObjectId id, CancellationToken cancellationToken);

    Task DeleteAsync(ObjectId id, CancellationToken cancellationToken);

    Task<List<Template>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken);
}

public class TemplateRepository(IMongoClient client) : ITemplateRepository
{
    public async Task CreateAsync(Template template, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        await collection.InsertOneAsync(template, cancellationToken: cancellationToken);
    }

    public Task<List<Template>> GetAllAsync(CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.Find(template => true).ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<Template> GetByIdAsync(ObjectId id, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.Find(template => template.Id == id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(ObjectId id, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.DeleteOneAsync(template => template.Id == id, cancellationToken: cancellationToken);
    }

    public Task<List<Template>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.Find(template => template.PersonId == personId)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    private IMongoCollection<Template> GetCollection()
    {
        var database = client.GetDatabase(MongoDbConstants.DatabaseName);
        return database.GetCollection<Template>(MongoDbConstants.TemplateCollectionName);
    }
}