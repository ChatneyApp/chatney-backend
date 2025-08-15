using MongoDB.Driver;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    public async Task<Config> AddConfig(IMongoDatabase mongoDatabase, ConfigDTO configDto)
    {
        var collection = mongoDatabase.GetCollection<Config>("system_config");
        Config config = Config.FromDTO(configDto);
        await collection.InsertOneAsync(config);
        return config;
    }

    // TODO: ignore type field - it's constant for system config
    public async Task<Config?> UpdateConfig(IMongoDatabase mongoDatabase, Config config)
    {
        var collection = mongoDatabase.GetCollection<Config>("system_config");
        var filter = Builders<Config>.Filter.Eq("_id", config.Id);
        var result = await collection.ReplaceOneAsync(filter, config);
        return result.ModifiedCount > 0 ? config : null;
    }

    public async Task<bool> DeleteConfig(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Config>("system_config");
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
