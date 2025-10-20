using MongoDB.Driver;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    // TODO: ignore type field - it's constant for system config
    public async Task<Config?> UpdateConfig(IMongoDatabase mongoDatabase, Config config)
    {
        var collection = mongoDatabase.GetCollection<Config>(DomainSettings.ConfigCollectionName);
        var filter = Builders<Config>.Filter.Eq("_id", config.Id);
        var result = await collection.ReplaceOneAsync(filter, config);
        return result.ModifiedCount > 0 ? config : null;
    }
}
