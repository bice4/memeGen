using MemeGen.ClientApiService.Models;
using MemeGen.Common.Constants;
using MemeGen.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MemeGen.ClientApiService.Persistent.MongoDb;

public interface ITemplateRepository
{
    Task<List<Template>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken);

    Task<List<TemplatePerson>> GetPersonsFromTemplatesAsync(CancellationToken cancellationToken);
    
    Task IncreaseTemplateUsageAsync(ObjectId templateId, CancellationToken cancellationToken);
}

public class TemplateRepository(IMongoClient client) : ITemplateRepository
{
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