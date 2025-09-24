using MemeGen.Domain.Entities;
using MemeGen.MongoDbService.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MemeGen.MongoDbService.Repositories;

public interface ITemplateRepository
{
    Task<List<Template>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken);

    Task<List<TemplatePerson>> GetPersonsFromTemplatesAsync(CancellationToken cancellationToken);

    Task IncreaseTemplateUsageAsync(ObjectId templateId, CancellationToken cancellationToken);

    Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken);

    Task CreateAsync(Template template, CancellationToken cancellationToken);

    Task<List<Template>> GetAllAsync(CancellationToken cancellationToken);

    Task DeleteAsync(ObjectId id, CancellationToken cancellationToken);
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
    
    public Task DeleteAsync(ObjectId id, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.DeleteOneAsync(template => template.Id == id, cancellationToken: cancellationToken);
    }

    public Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.Find(template => template.Id == id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)!;
    }

    public Task<List<Template>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        return collection.Find(template => template.PersonId == personId)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<TemplatePerson>> GetPersonsFromTemplatesAsync(CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        var templates = await collection.Find(template => true).ToListAsync(cancellationToken: cancellationToken);

        return templates.GroupBy(x => x.PersonId)
            .Select(g
                => new TemplatePerson(g.Key, g.First().PersonName))
            .ToList();
    }

    public Task IncreaseTemplateUsageAsync(ObjectId templateId, CancellationToken cancellationToken)
    {
        var collection = GetCollection();
        var filter = Builders<Template>.Filter.Eq(x => x.Id, templateId);
        var update = Builders<Template>.Update.Inc(x => x.Usages, 1);
        return collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    private IMongoCollection<Template> GetCollection()
    {
        var database = client.GetDatabase(MongoDbConstants.DatabaseName);
        return database.GetCollection<Template>(MongoDbConstants.TemplateCollectionName);
    }
}