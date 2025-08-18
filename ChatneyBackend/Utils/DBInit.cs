using ChatneyBackend.Domains.Users;
using MongoDB.Driver;
using UsersDomainSettings = ChatneyBackend.Domains.Users.DomainSettings;

namespace ChatneyBackend.Utils;

public static class DbInit
{
    public static void Init(MongoClient mongoClient, string dbName)
    {
        var db = mongoClient.GetDatabase(dbName);
        // == Users collection initialization ==
        var usersCollection = db.GetCollection<User>(UsersDomainSettings.UserCollectionName);
        // Create unique index on email field
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexModel = new CreateIndexModel<User>(emailIndex, indexOptions);
        usersCollection.Indexes.CreateOne(indexModel);

        // TODO: add default system_config values
    }
}
