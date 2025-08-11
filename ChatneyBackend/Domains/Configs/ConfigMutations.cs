using MongoDB.Driver;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    public Config AddConfig(IMongoDatabase mongoDatabase, Config config)
    {
        var collection = mongoDatabase.GetCollection<Config>("system_config");
        collection.InsertOne(config);
        return config;
    }

    public bool DeleteConfig(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Config>("system_config");
        var result = collection.DeleteOne(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
