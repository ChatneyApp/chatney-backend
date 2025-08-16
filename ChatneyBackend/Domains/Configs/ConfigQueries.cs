using MongoDB.Driver;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
    public async Task<Config?> GetConfigById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Config>(DomainSettings.ConfigCollectionName);
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    public async Task<Config?> GetConfigByName(IMongoDatabase mongoDatabase, string name)
    {
        var collection = mongoDatabase.GetCollection<Config>(DomainSettings.ConfigCollectionName);
        var records = (await collection.FindAsync(r => r.Name == name)).ToList();
        return records.Count > 0
            ? records[0]
            : null;

    }

    public async Task<List<Config>> GetList(IMongoDatabase mongoDatabase)
    {
        var collection = mongoDatabase.GetCollection<Config>(DomainSettings.ConfigCollectionName);
        var records = await collection.FindAsync(Builders<Config>.Filter.Empty);
        return records.ToList();
    }
}
