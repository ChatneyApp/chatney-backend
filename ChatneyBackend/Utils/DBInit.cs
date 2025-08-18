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
        var emailIndexOptions = new CreateIndexOptions { Unique = true };
        var emailIndexModel = new CreateIndexModel<User>(emailIndex, emailIndexOptions);

        var nameIndex = Builders<User>.IndexKeys.Ascending(u => u.Name);
        var nameIndexOptions = new CreateIndexOptions { Unique = true };
        var nameIndexModel = new CreateIndexModel<User>(nameIndex, nameIndexOptions);
        usersCollection.Indexes.CreateMany([nameIndexModel, emailIndexModel]);
    }
}
