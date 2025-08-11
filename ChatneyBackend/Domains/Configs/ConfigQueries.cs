using MongoDB.Driver;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
    public Config GetConfigById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<Config>("system_config").Find(c => c.Id == id).First();

    public Config GetConfigByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<Config>("system_config").Find(c => c.Name == name).First();

    public List<Config> GetList(IMongoDatabase mongoDatabase)
        => mongoDatabase.GetCollection<Config>("system_config").Find(Builders<Config>.Filter.Empty).ToList();
}
